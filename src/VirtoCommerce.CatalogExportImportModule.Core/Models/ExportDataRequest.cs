namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public class ExportDataRequest
    {
        public string DataType { get; set; }
        public string CatalogId { get; set; }
        public string CategoryId { get; set; }
        public string[] CategoryIds { get; set; }

        public string[] ItemIds { get; set; }

        public string Keyword { get; set; }
    }
}
