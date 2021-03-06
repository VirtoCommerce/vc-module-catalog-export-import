using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class CsvPagedEditorialReviewDataImporter : CsvPagedDataImporter<CsvEditorialReview>
    {
        private readonly ICsvValidatorFactory _csvValidatorFactory;
        private readonly IValidator<ImportRecord<CsvEditorialReview>[]> _importRecordsValidator;
        private readonly IProductSearchService _productSearchService;
        private readonly IItemService _itemService;
        private readonly IProductEditorialReviewService _editorialReviewService;

        public CsvPagedEditorialReviewDataImporter(
            IBlobUrlResolver blobUrlResolver,
            IImportConfigurationFactory importConfigurationFactory, IImportPagedDataSourceFactory dataSourceFactory,
            ICsvValidatorFactory csvValidatorFactory, IValidator<ImportRecord<CsvEditorialReview>[]> importRecordsValidator,
            ICsvParsingErrorHandlerFactory parsingErrorHandlerFactory, ICsvImportErrorReporterFactory importReporterFactory, 
            IProductSearchService productSearchService, IItemService itemService, IProductEditorialReviewService editorialReviewService)
            : base(blobUrlResolver, importConfigurationFactory, dataSourceFactory, parsingErrorHandlerFactory, importReporterFactory)
        {
            _csvValidatorFactory = csvValidatorFactory;
            _importRecordsValidator = importRecordsValidator;
            _itemService = itemService;
            _productSearchService = productSearchService;
            _editorialReviewService = editorialReviewService;
        }

        public override string DataType => nameof(EditorialReview);


        protected override async Task ProcessChunkAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback,
            IImportPagedDataSource<CsvEditorialReview> dataSource, ICsvImportErrorReporter importErrorReporter, ImportProgressInfo importProgress)
        {
            var importReviewRecords = dataSource.Items;

            try
            {
                var reviewsIds = importReviewRecords.Select(x => x.Record?.DescriptionId).Distinct()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                var existedReviews = (await _editorialReviewService.GetByIdsAsync(reviewsIds)).OfType<ExtendedEditorialReview>().ToArray();

                SetIdAndSkuToNullByExistence(importReviewRecords, existedReviews);

                var validationContext = new ValidationContext<ImportRecord<CsvEditorialReview>[]>(importReviewRecords);

                var validator = _csvValidatorFactory.Create(_importRecordsValidator, importErrorReporter);
                var validationResult = validator.Validate(validationContext);

                var invalidImportRecords = validationResult.Errors
                    .Select(x => (x.CustomState as ImportValidationState<CsvEditorialReview>)?.InvalidRecord).Distinct().ToArray();

                importReviewRecords = importReviewRecords.Except(invalidImportRecords).ToArray();

                var productsSkuArray = importReviewRecords.Select(x => x.Record?.ProductSku).Distinct()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                var allProductsForSaving = new List<CatalogProduct>();

                var importReviewsForAdding = importReviewRecords.Where(x => !string.IsNullOrEmpty(x.Record.ProductSku))
                    .Select(x => x.Record).ToArray();

                if (importReviewsForAdding.Length > 0)
                {
                    var productSearchResult = await _productSearchService.SearchProductsAsync(
                        new ProductSearchCriteria() { Skus = productsSkuArray, ResponseGroup = ItemResponseGroup.ItemEditorialReviews.ToString(), Take = int.MaxValue });

                    var productsForReviewAdding = productSearchResult.Results;

                    AddReviewsToProducts(productsForReviewAdding, importReviewsForAdding);

                    allProductsForSaving.AddRange(productsForReviewAdding);
                }

                var importReviewsForUpdating = importReviewRecords.Where(x => !string.IsNullOrEmpty(x.Record.DescriptionId))
                    .Select(x => x.Record).ToArray();

                if (importReviewsForUpdating.Length > 0)
                {
                    var productIds = existedReviews.Select(x => x.ItemId).ToArray();

                    var productsForReviewUpdating = await _itemService.GetByIdsAsync(productIds, respGroup: ItemResponseGroup.ItemEditorialReviews.ToString());

                    CorrectProductsForUpdatingWithForAdding(productsForReviewUpdating, allProductsForSaving);

                    PatchProductsReviews(productsForReviewUpdating, importReviewsForUpdating);

                    allProductsForSaving.AddRange(productsForReviewUpdating);
                }

                await _itemService.SaveChangesAsync(allProductsForSaving.ToArray());


                importProgress.CreatedCount += importReviewsForAdding.Length;
                importProgress.UpdatedCount += importReviewsForUpdating.Length;
            }
            catch (Exception e)
            {
                importProgress.Errors.Add(e.Message);
                progressCallback(importProgress);
            }
            finally
            {
                importProgress.ProcessedCount = Math.Min(dataSource.CurrentPageNumber * dataSource.PageSize,
                    importProgress.TotalCount);
                importProgress.ErrorCount = importProgress.ProcessedCount - importProgress.CreatedCount -
                                            importProgress.UpdatedCount;
            }
        }

        private static void CorrectProductsForUpdatingWithForAdding(CatalogProduct[] productsForReviewUpdating, List<CatalogProduct> allProductsForSaving)
        {
            if (allProductsForSaving.Count > 0)
            {
                for (var i = 0; i < productsForReviewUpdating.Length; i++)
                {
                    var updateProduct = productsForReviewUpdating[i];
                    var addProduct = allProductsForSaving.FirstOrDefault(x => x.Id.EqualsInvariant(updateProduct.Id));

                    if (addProduct != null)
                    {
                        productsForReviewUpdating[i] = addProduct;
                        allProductsForSaving.Remove(addProduct);
                    }
                }
            }
        }


        private static void SetIdAndSkuToNullByExistence(ImportRecord<CsvEditorialReview>[] importReviewRecords, ExtendedEditorialReview[] existedReviews)
        {
            foreach (var importRecord in importReviewRecords)
            {
                if (existedReviews.Any(x => x.Id.EqualsInvariant(importRecord.Record.DescriptionId)))
                {
                    importRecord.Record.ProductSku = null;
                }
                else
                {
                    importRecord.Record.DescriptionId = null;
                }
            }
        }

        private static void AddReviewsToProducts(IList<CatalogProduct> productsForReviewAdding, CsvEditorialReview[] importReviewsForAdding)
        {
            foreach (var product in productsForReviewAdding)
            {
                foreach (var productImportReview in importReviewsForAdding.Where(x => x.ProductSku.EqualsInvariant(product.Code)))
                {
                    var newReview = AbstractTypeFactory<EditorialReview>.TryCreateInstance<EditorialReview>();
                    productImportReview.PatchModel(newReview);
                    product.Reviews.Add(newReview);
                }
            }
        }

        private static void PatchProductsReviews(IList<CatalogProduct> productsForReviewUpdating, CsvEditorialReview[] importReviewForUpdating)
        {
            foreach (var product in productsForReviewUpdating)
            {
                foreach (var review in product.Reviews)
                {
                    var importReviewForUpdate =
                        importReviewForUpdating.LastOrDefault(x => x.DescriptionId.EqualsInvariant(review.Id));

                    importReviewForUpdate?.PatchModel(review);
                }
            }
        }
    }
}
