using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class CsvPagedReviewDataImporter : CsvPagedDataImporter<CsvEditorialReview>
    {
        private readonly IProductSearchService _productSearchService;

        public CsvPagedReviewDataImporter(IImportPagedDataSourceFactory dataSourceFactory,
            IValidator<ImportRecord<CsvEditorialReview>[]> importRecordsValidator,
            ICsvImportReporterFactory importReporterFactory, IBlobUrlResolver blobUrlResolver,
            IProductSearchService productSearchService)
            : base(dataSourceFactory, importRecordsValidator, importReporterFactory, blobUrlResolver)
        {
        }

        public override string MemberType => nameof(EditorialReview);

        protected override async Task ProcessChunkAsync(ImportDataRequest request,
            Action<ImportProgressInfo> progressCallback, IImportPagedDataSource<CsvEditorialReview> dataSource,
            ImportErrorsContext errorsContext, ImportProgressInfo importProgress, ICsvImportReporter importReporter)
        {
            var importReview = dataSource.Items
                // expect records that was parsed with errors
                .Where(importContact => !errorsContext.ErrorsRows.Contains(importContact.Row))
                .ToArray();

            try
            {
                var reviewsIds = importReview.Select(x => x.Record?.Id).Distinct()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                var productsSkuArray = importReview.Select(x => x.Record?.ProductSku).Distinct()
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();

                var productSearchResult = _productSearchService.SearchProductsAsync(
                    new ProductSearchCriteria() { Skus = productsSkuArray, ResponseGroup = ItemResponseGroup.ItemEditorialReviews.ToString() });


                //importProgress.CreatedCount += newContacts.Length;
                //importProgress.UpdatedCount += existedContacts.Length;
            }
            catch (Exception e)
            {
                HandleError(progressCallback, importProgress, e.Message);
            }
            finally
            {
                importProgress.ProcessedCount = Math.Min(dataSource.CurrentPageNumber * dataSource.PageSize,
                    importProgress.TotalCount);
                importProgress.ErrorCount = importProgress.ProcessedCount - importProgress.CreatedCount -
                                            importProgress.UpdatedCount;
            }
        }
    }
}
