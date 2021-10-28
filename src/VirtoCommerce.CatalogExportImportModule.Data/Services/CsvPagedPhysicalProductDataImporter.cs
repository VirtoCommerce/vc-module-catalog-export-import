using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class CsvPagedPhysicalProductDataImporter : CsvPagedDataImporter<CsvPhysicalProduct>
    {
        private readonly IImportProductSearchService _importProductSearchService;
        private readonly IImportCategorySearchService _importCategorySearchService;
        private readonly IItemService _itemService;
        private readonly IImportProductsClassMapFactory _classMapFactory;

        public CsvPagedPhysicalProductDataImporter(
            IImportPagedDataSourceFactory dataSourceFactory, IValidator<ImportRecord<CsvPhysicalProduct>[]> importRecordsValidator,
            ICsvImportReporterFactory importReporterFactory, IBlobUrlResolver blobUrlResolver,
            IImportProductSearchService importProductSearchService, IImportCategorySearchService importCategorySearchService,
            IItemService itemService, ImportConfigurationFactory importConfigurationFactory,
            IImportProductsClassMapFactory classMapFactory)
            : base(dataSourceFactory, importRecordsValidator, importReporterFactory, blobUrlResolver, importConfigurationFactory)
        {
            _importProductSearchService = importProductSearchService;
            _importCategorySearchService = importCategorySearchService;
            _itemService = itemService;
            _classMapFactory = classMapFactory;
        }

        public override string DataType { get; } = ModuleConstants.DataTypes.PhysicalProduct;

        protected override async Task<ClassMap<CsvPhysicalProduct>> GetClassMapAsync(ImportDataRequest request)
        {
            var classMap = await _classMapFactory.CreateClassMapAsync(request.CatalogId);

            return classMap;
        }

        protected override async Task ProcessChunkAsync(ImportDataRequest request,
            Action<ImportProgressInfo> progressCallback, IImportPagedDataSource<CsvPhysicalProduct> dataSource,
            ImportErrorsContext errorsContext, ImportProgressInfo importProgress, ICsvImportReporter importReporter)
        {
            var records = dataSource.Items
                // expect records that was parsed with errors
                .Where(importContact => !errorsContext.Errors.Select(error => error.Row).Contains(importContact.Row))
                .ToArray();

            try
            {
                var internalIds = records.Select(x => x.Record?.ProductId).Distinct()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                var outerIds = records.Select(x => x.Record?.ProductOuterId).Distinct()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                var existingProducts = await SearchProductsByIdAndOuterIdAsync(internalIds, outerIds);

                SetIdToNullForNotExisting(records, existingProducts);

                SetIdToRealForExistingOuterId(records, existingProducts);

                var internalCategoryIds = records.Select(x => x.Record?.CategoryId).Distinct()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                var outerCategoryIds = records.Select(x => x.Record?.CategoryOuterId).Distinct()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                var existingCategories = await SearchCategoriesByIdAndOuterIdAsync(internalCategoryIds, outerCategoryIds);

                SetCategoryIdByCategoryOuterId(records, existingCategories);

                var validationResult = await ValidateAsync(records, errorsContext);

                var invalidRecords = validationResult.Errors
                    .Select(x => (x.CustomState as ImportValidationState<CsvPhysicalProduct>)?.InvalidRecord).Distinct().ToArray();

                records = records.Except(invalidRecords).ToArray();

                existingProducts = existingProducts.Where(existingEntity => records.Any(record =>
                        existingEntity.Id.EqualsInvariant(record.Record.ProductId)
                        || !string.IsNullOrEmpty(existingEntity.OuterId) && existingEntity.OuterId.EqualsInvariant(record.Record.ProductOuterId)))
                    .ToArray();

                var recordsToUpdate = records.Where(record => existingProducts.Any(existingEntity =>
                    existingEntity.Id.EqualsInvariant(record.Record.ProductId)
                    || !existingEntity.OuterId.IsNullOrEmpty() && existingEntity.OuterId.EqualsInvariant(record.Record.ProductOuterId))
                ).ToArray();

                existingProducts = GetReducedExistedByWrongOuterId(recordsToUpdate, existingProducts);

                var recordsToCreate = records.Except(recordsToUpdate).ToArray();

                var newProducts = CreateNewProducts(recordsToCreate);

                PatchExistingProducts(existingProducts, recordsToUpdate);

                var productsToSave = newProducts.Union(existingProducts).ToArray();

                SetCatalogId(productsToSave, request.CatalogId);

                await _itemService.SaveChangesAsync(productsToSave);

                importProgress.CreatedCount += newProducts.Length;
                importProgress.UpdatedCount += existingProducts.Length;
            }
            catch (Exception e)
            {
                HandleError(progressCallback, importProgress, e.Message);
            }
            finally
            {
                importProgress.ProcessedCount = Math.Min(dataSource.CurrentPageNumber * dataSource.PageSize, importProgress.TotalCount);
                importProgress.ErrorCount = importProgress.ProcessedCount - importProgress.CreatedCount - importProgress.UpdatedCount;
            }
        }

        private async Task<CatalogProduct[]> SearchProductsByIdAndOuterIdAsync(string[] internalIds, string[] outerIds)
        {
            var criteriaById = new ImportSearchCriteria
            {
                ObjectIds = internalIds,
                Skip = 0,
                Take = ModuleConstants.Settings.PageSize
            };

            var productsById = internalIds.IsNullOrEmpty() ? Array.Empty<CatalogProduct>() : (await _importProductSearchService.SearchAsync(criteriaById)).Results;

            var criteriaByOuterId = new ImportSearchCriteria
            {
                OuterIds = outerIds,
                Skip = 0,
                Take = ModuleConstants.Settings.PageSize
            };

            var productsByOuterId = outerIds.IsNullOrEmpty() ? Array.Empty<CatalogProduct>() : (await _importProductSearchService.SearchAsync(criteriaByOuterId)).Results;

            var existingProducts = productsById.Union(productsByOuterId, AnonymousComparer.Create<CatalogProduct>((x, y) => x.Id == y.Id, x => x.Id.GetHashCode())).ToArray();

            return existingProducts;
        }

        private async Task<Category[]> SearchCategoriesByIdAndOuterIdAsync(string[] internalCategoryIds, string[] outerCategoryIds)
        {
            var criteriaById = new ImportSearchCriteria
            {
                ObjectIds = internalCategoryIds,
                Skip = 0,
                Take = ModuleConstants.Settings.PageSize
            };

            var categoriesById = internalCategoryIds.IsNullOrEmpty() ? Array.Empty<Category>() : (await _importCategorySearchService.SearchAsync(criteriaById)).Results;

            var criteriaByOuterId = new ImportSearchCriteria
            {
                OuterIds = outerCategoryIds,
                Skip = 0,
                Take = ModuleConstants.Settings.PageSize
            };

            var categoriesByOuterId = outerCategoryIds.IsNullOrEmpty() ? Array.Empty<Category>() : (await _importCategorySearchService.SearchAsync(criteriaByOuterId)).Results;

            var existingCategories = categoriesById.Union(categoriesByOuterId, AnonymousComparer.Create<Category>((x, y) => x.Id == y.Id, x => x.Id.GetHashCode())).ToArray();

            return existingCategories;
        }

        /// <summary>
        /// Set id to null for records that's not existed in the system. It reduce count of wrong duplicates.
        /// All such records will be created if they are valid. 
        /// </summary>
        /// <param name="records"></param>
        /// <param name="existingProducts"></param>
        private static void SetIdToNullForNotExisting(ImportRecord<CsvPhysicalProduct>[] records, CatalogProduct[] existingProducts)
        {
            foreach (var record in records)
            {
                var existingEntity =
                    existingProducts.FirstOrDefault(x => x.Id.EqualsInvariant(record.Record.ProductId));

                if (existingEntity == null)
                {
                    record.Record.ProductId = null;
                }
            }
        }

        /// <summary>
        /// Set id for import records to the real existed value when the system record was found by outer id.
        /// It allow us to find duplicates not only by outer id but by id also for such records. 
        /// </summary>
        /// <param name="records"></param>
        /// <param name="existingProducts"></param>
        private static void SetIdToRealForExistingOuterId(ImportRecord<CsvPhysicalProduct>[] records, CatalogProduct[] existingProducts)
        {
            foreach (var record in records.Where(x => string.IsNullOrEmpty(x.Record.ProductId) && !string.IsNullOrEmpty(x.Record.ProductOuterId)))
            {
                var existedEntity =
                    existingProducts.FirstOrDefault(x => !string.IsNullOrEmpty(x.OuterId) && x.OuterId.EqualsInvariant(record.Record.ProductOuterId));

                if (existedEntity != null)
                {
                    record.Record.ProductId = existedEntity.Id;
                }
            }
        }

        private static void SetCategoryIdByCategoryOuterId(ImportRecord<CsvPhysicalProduct>[] records, Category[] existingCategories)
        {
            foreach (var record in records.Where(x => string.IsNullOrEmpty(x.Record.CategoryId) && !string.IsNullOrEmpty(x.Record.CategoryOuterId)))
            {
                var existingCategory =
                    existingCategories.FirstOrDefault(x => !string.IsNullOrEmpty(x.OuterId) && x.OuterId.EqualsInvariant(record.Record.CategoryOuterId));

                if (existingCategory != null)
                {
                    record.Record.CategoryId = existingCategory.Id;
                }
            }
        }

        /// <summary>
        /// Reduce existed members list. Some records may have been mistakenly selected for updating. When id and outer id from a file refs to different records in the system.
        /// In that case record with outer id should be excepted from the list to updating. 
        /// </summary>
        /// <param name="records"></param>
        /// <param name="existingProducts"></param>
        /// <returns></returns>
        private CatalogProduct[] GetReducedExistedByWrongOuterId(ImportRecord<CsvPhysicalProduct>[] records, CatalogProduct[] existingProducts)
        {
            var result = existingProducts;

            foreach (var record in records.Where(x => !string.IsNullOrEmpty(x.Record.ProductId) && !string.IsNullOrEmpty(x.Record.ProductOuterId)))
            {
                var otherExisted = existingProducts.FirstOrDefault(x => !x.Id.EqualsInvariant(record.Record.ProductId) && x.OuterId.EqualsInvariant(record.Record.ProductOuterId));

                if (otherExisted != null && !records.Any(x =>
                    x.Record.ProductOuterId.EqualsInvariant(otherExisted.OuterId) && (x.Record.ProductId.EqualsInvariant(otherExisted.Id) || string.IsNullOrEmpty(x.Record.ProductId))))
                {
                    result = result.Except(new[] { otherExisted }).ToArray();
                }
            }

            return result;
        }

        private static CatalogProduct[] CreateNewProducts(ImportRecord<CsvPhysicalProduct>[] records)
        {
            var newProducts = records.Select(record =>
            {
                var entity = AbstractTypeFactory<CatalogProduct>.TryCreateInstance();

                record.Record.PatchModel(entity);

                var reviewEntity = AbstractTypeFactory<EditorialReview>.TryCreateInstance();
                record.Record.PatchDescription(reviewEntity);
                entity.Reviews = new List<EditorialReview> { reviewEntity };

                return entity;
            }).ToArray();

            return newProducts;
        }

        private static void PatchExistingProducts(IEnumerable<CatalogProduct> existingProducts, ImportRecord<CsvPhysicalProduct>[] records)
        {
            foreach (var existingEntity in existingProducts)
            {
                var record = records.LastOrDefault(x => existingEntity.Id.EqualsInvariant(x.Record.ProductId)
                                                        || !existingEntity.OuterId.IsNullOrEmpty() && existingEntity.OuterId.EqualsInvariant(x.Record.ProductOuterId));

                record?.Record.PatchModel(existingEntity);

                var existedReview = existingEntity.Reviews.FirstOrDefault(x => x.Id == record.Record.DescriptionId);

                if (existedReview != null)
                {
                    record.Record.PatchDescription(existedReview);
                } else
                {
                    var review = AbstractTypeFactory<EditorialReview>.TryCreateInstance();
                    record.Record.PatchDescription(review);
                    existingEntity.Reviews = new List<EditorialReview> { review };
                }
            }
        }

        private void SetCatalogId(CatalogProduct[] productsToSave, string catalogId)
        {
            foreach (var product in productsToSave)
            {
                product.CatalogId = catalogId;
            }
        }
    }
}
