namespace VirtoCommerce.DescriptionExportImportModule.Core.Models
{
    public class ExportDataRequest
    {
        public string CatalogId { get; set; }
        public string CategoryId { get; set; }

        public ProductEditorialReviewSearchCriteria ToSearchCriteria()
        {
            return new ProductEditorialReviewSearchCriteria
            {
                CatalogId = CatalogId,
                CategoryId = CategoryId,
            };
        }
    }
}
