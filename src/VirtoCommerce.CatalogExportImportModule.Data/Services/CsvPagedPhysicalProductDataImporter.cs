using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
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

        public CsvPagedPhysicalProductDataImporter(
            IImportPagedDataSourceFactory dataSourceFactory, IValidator<ImportRecord<CsvPhysicalProduct>[]> importRecordsValidator,
            ICsvImportReporterFactory importReporterFactory, IBlobUrlResolver blobUrlResolver,
            IImportProductSearchService importProductSearchService, IImportCategorySearchService importCategorySearchService,
            IItemService itemService)
            : base(dataSourceFactory, importRecordsValidator, importReporterFactory, blobUrlResolver)
        {
            _importProductSearchService = importProductSearchService;
            _importCategorySearchService = importCategorySearchService;
            _itemService = itemService;
        }

        public override string DataType { get; } = nameof(CatalogProduct);

        protected override async Task ProcessChunkAsync(ImportDataRequest request,
            Action<ImportProgressInfo> progressCallback, IImportPagedDataSource<CsvPhysicalProduct> dataSource,
            ImportErrorsContext errorsContext, ImportProgressInfo importProgress, ICsvImportReporter importReporter)
        {
            var records = dataSource.Items
                // expect records that was parsed with errors
                .Where(importContact => !errorsContext.ErrorsRows.Contains(importContact.Row))
                .ToArray();

            try
            {
                var internalIds = records.Select(x => x.Record?.ProductId).Distinct()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                var outerIds = records.Select(x => x.Record?.ProductOuterId).Distinct()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                var existingEntities = await SearchEntitiesByIdAndOuterIdAsync(internalIds, outerIds);

                SetIdToNullForNotExisting(records, existingEntities);

                SetIdToRealForExistingOuterId(records, existingEntities);

                var internalCategoryIds = records.Select(x => x.Record?.CategoryId).Distinct()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                var outerCategoryIds = records.Select(x => x.Record?.CategoryOuterId).Distinct()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                var existingCategoryEntities = await SearchCategoriesByIdAndOuterIdAsync(internalCategoryIds, outerCategoryIds);

                SetCategoryIdByCategoryOuterId(records, existingCategoryEntities);

                var validationResult = await ValidateAsync(records, importReporter);

                var invalidRecords = validationResult.Errors
                    .Select(x => (x.CustomState as ImportValidationState<CsvPhysicalProduct>)?.InvalidRecord).Distinct().ToArray();

                records = records.Except(invalidRecords).ToArray();
                
                existingEntities = existingEntities.Where(existingEntity => records.Any(record =>
                        existingEntity.Id.EqualsInvariant(record.Record.ProductId)
                        || !string.IsNullOrEmpty(existingEntity.OuterId) && existingEntity.OuterId.EqualsInvariant(record.Record.ProductOuterId)))
                    .ToArray();

                var recordsToUpdate = records.Where(record => existingEntities.Any(existingEntity =>
                    existingEntity.Id.EqualsInvariant(record.Record.ProductId)
                    || !existingEntity.OuterId.IsNullOrEmpty() && existingEntity.OuterId.EqualsInvariant(record.Record.ProductOuterId))
                ).ToArray();

                existingEntities = GetReducedExistedByWrongOuterId(recordsToUpdate, existingEntities);

                var recordsToCreate = records.Except(recordsToUpdate).ToArray();

                var newEntities = CreateNewEntities(recordsToCreate);

                PatchExistingEntities(existingEntities, recordsToUpdate);

                var entitiesToSave = newEntities.Union(existingEntities).ToArray();

                await _itemService.SaveChangesAsync(entitiesToSave);

                importProgress.CreatedCount += newEntities.Length;
                importProgress.UpdatedCount += existingEntities.Length;
            }
            catch(Exception e)
            {
                HandleError(progressCallback, importProgress, e.Message);
            }
            finally
            {
                importProgress.ProcessedCount = Math.Min(dataSource.CurrentPageNumber * dataSource.PageSize, importProgress.TotalCount);
                importProgress.ErrorCount = importProgress.ProcessedCount - importProgress.CreatedCount - importProgress.UpdatedCount;
            }
        }

        private async Task<CatalogProduct[]> SearchEntitiesByIdAndOuterIdAsync(string[] internalIds, string[] outerIds)
        {
            var criteriaById = new ImportSearchCriteria
            {
                ObjectIds = internalIds,
                Skip = 0,
                Take = ModuleConstants.Settings.PageSize
            };

            var entitiesById = internalIds.IsNullOrEmpty() ? Array.Empty<CatalogProduct>() : (await _importProductSearchService.SearchAsync(criteriaById)).Results;

            var criteriaByOuterId = new ImportSearchCriteria
            {
                OuterIds = outerIds,
                Skip = 0,
                Take = ModuleConstants.Settings.PageSize
            };

            var entitiesByOuterId = outerIds.IsNullOrEmpty() ? Array.Empty<CatalogProduct>() : (await _importProductSearchService.SearchAsync(criteriaByOuterId)).Results;

            var existingEntities = entitiesById.Union(entitiesByOuterId, AnonymousComparer.Create<CatalogProduct>((x, y) => x.Id == y.Id, x => x.Id.GetHashCode())).ToArray();

            return existingEntities;
        }

        private async Task<Category[]> SearchCategoriesByIdAndOuterIdAsync(string[] internalCategoryIds, string[] outerCategoryIds)
        {
            var criteriaById = new ImportSearchCriteria
            {
                ObjectIds = internalCategoryIds,
                Skip = 0,
                Take = ModuleConstants.Settings.PageSize
            };

            var entitiesById = internalCategoryIds.IsNullOrEmpty() ? Array.Empty<Category>() : (await _importCategorySearchService.SearchAsync(criteriaById)).Results;

            var criteriaByOuterId = new ImportSearchCriteria
            {
                OuterIds = outerCategoryIds,
                Skip = 0,
                Take = ModuleConstants.Settings.PageSize
            };

            var entitiesByOuterId = outerCategoryIds.IsNullOrEmpty() ? Array.Empty<Category>() : (await _importCategorySearchService.SearchAsync(criteriaByOuterId)).Results;

            var existingEntities = entitiesById.Union(entitiesByOuterId, AnonymousComparer.Create<Category>((x, y) => x.Id == y.Id, x => x.Id.GetHashCode())).ToArray();

            return existingEntities;
        }

        /// <summary>
        /// Set id to null for records that's not existed in the system. It reduce count of wrong duplicates.
        /// All such records will be created if they are valid. 
        /// </summary>
        /// <param name="records"></param>
        /// <param name="existingEntities"></param>
        private static void SetIdToNullForNotExisting(ImportRecord<CsvPhysicalProduct>[] records, CatalogProduct[] existingEntities)
        {
            foreach (var record in records)
            {
                var existingEntity =
                    existingEntities.FirstOrDefault(x => x.Id.EqualsInvariant(record.Record.ProductId));

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
        /// <param name="existingEntities"></param>
        private static void SetIdToRealForExistingOuterId(ImportRecord<CsvPhysicalProduct>[] records, CatalogProduct[] existingEntities)
        {
            foreach (var record in records.Where(x => string.IsNullOrEmpty(x.Record.ProductId) && !string.IsNullOrEmpty(x.Record.ProductOuterId)))
            {
                var existedEntity =
                    existingEntities.FirstOrDefault(x => !string.IsNullOrEmpty(x.OuterId) && x.OuterId.EqualsInvariant(record.Record.ProductOuterId));

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
                    record.Record.ProductId = existingCategory.Id;
                }
            }
        }

        /// <summary>
        /// Reduce existed members list. Some records may have been mistakenly selected for updating. When id and outer id from a file refs to different records in the system.
        /// In that case record with outer id should be excepted from the list to updating. 
        /// </summary>
        /// <param name="records"></param>
        /// <param name="existingEntities"></param>
        /// <returns></returns>
        private CatalogProduct[] GetReducedExistedByWrongOuterId(ImportRecord<CsvPhysicalProduct>[] records, CatalogProduct[] existingEntities)
        {
            var result = existingEntities;

            foreach (var record in records.Where(x => !string.IsNullOrEmpty(x.Record.ProductId) && !string.IsNullOrEmpty(x.Record.ProductOuterId)))
            {
                var otherExisted = existingEntities.FirstOrDefault(x => !x.Id.EqualsInvariant(record.Record.ProductId) && x.OuterId.EqualsInvariant(record.Record.ProductOuterId));

                if (otherExisted != null && !records.Any(x =>
                    x.Record.ProductOuterId.EqualsInvariant(otherExisted.OuterId) && (x.Record.ProductId.EqualsInvariant(otherExisted.Id) || string.IsNullOrEmpty(x.Record.ProductId))))
                {
                    result = result.Except(new[] { otherExisted }).ToArray();
                }
            }

            return result;
        }

        private static CatalogProduct[] CreateNewEntities(ImportRecord<CsvPhysicalProduct>[] records)
        {
            var newEntities = records.Select(record =>
            {
                var entity = AbstractTypeFactory<CatalogProduct>.TryCreateInstance();

                record.Record.PatchModel(entity);

                return entity;
            }).ToArray();

            return newEntities;
        }

        private static void PatchExistingEntities(IEnumerable<CatalogProduct> existingEntities, ImportRecord<CsvPhysicalProduct>[] records)
        {
            foreach (var existingEntity in existingEntities)
            {
                var record = records.LastOrDefault(x => existingEntity.Id.EqualsInvariant(x.Record.ProductId)
                                                        || !existingEntity.OuterId.IsNullOrEmpty() && existingEntity.OuterId.EqualsInvariant(x.Record.ProductOuterId));

                record?.Record.PatchModel(existingEntity);
            }
        }
    }
}
