using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogExportImportModule.Data.Helpers
{
    public static class ExportDataRequestExtensions
    {
        public static ProductEditorialReviewSearchCriteria ToProductEditorialReviewSearchCriteria(this ExportDataRequest dataRequest)
        {
            return new ProductEditorialReviewSearchCriteria
            {
                CatalogId = dataRequest.CatalogId,
                CategoryId = dataRequest.CategoryId,
                CategoryIds = dataRequest.CategoryIds,
                ItemIds = dataRequest.ItemIds,
                Keyword = dataRequest.Keyword,
                DeepSearch = true,
            };
        }

        public static CatalogListEntrySearchCriteria ToCatalogListEntrySearchCriteria(this ExportDataRequest dataRequest, int skip, int take, string objectType = null, string productType = null)
        {
            var result = new CatalogListEntrySearchCriteria()
            {
                ObjectType = objectType,
                ProductType = productType,

                CatalogId = dataRequest.CatalogId,
                CategoryId = dataRequest.CategoryId,
                Keyword = dataRequest.Keyword,
                SearchInChildren = true, // at index searching it search in children always. in this case flag means nothing. 
                SearchInVariations = false, // at index searching it search in variations always. in this case flag means nothing.
                Skip = skip,
                Take = take
            };

            return result;
        }
    }
}
