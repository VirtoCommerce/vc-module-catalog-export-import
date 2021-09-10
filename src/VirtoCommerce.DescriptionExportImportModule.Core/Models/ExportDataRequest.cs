namespace VirtoCommerce.DescriptionExportImportModule.Core.Models
{
    public class ExportDataRequest
    {
        public string CatalogId { get; set; }
        public string CategoryId { get; set; }
        public string[] CategoryIds { get; set; }

        public string[] ItemIds { get; set; }

        public string Keyword { get; set; }

        public ProductEditorialReviewSearchCriteria ToSearchCriteria()
        {
            return new ProductEditorialReviewSearchCriteria
            {
                CatalogId = CatalogId,
                CategoryId = CategoryId,
                CategoryIds = CategoryIds,
                ItemIds = ItemIds,
                Keyword = Keyword,
                DeepSearch = true,
            };
        }
    }
}
