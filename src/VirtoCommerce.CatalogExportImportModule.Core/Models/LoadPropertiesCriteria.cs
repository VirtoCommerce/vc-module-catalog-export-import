namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public class LoadPropertiesCriteria
    {
        public string CatalogId { get; set; }

        public string[] ItemIds { get; set; }

        public string[] CategoryIds { get; set; }
    }
}
