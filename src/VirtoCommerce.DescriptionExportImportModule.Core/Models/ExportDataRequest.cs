namespace VirtoCommerce.DescriptionExportImportModule.Core.Models
{
    public class ExportDataRequest
    {
        public string CatalogId { get; set; }
        public string CategoryId { get; set; }

        public ProductDescriptionSearchCriteria ToSearchCriteria()
        {
            return new ProductDescriptionSearchCriteria
            {
                CatalogId = CatalogId,
                CategoryId = CategoryId,
            };
        }
    }
}
