namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public sealed class ImportDataRequest
    {
        public string FilePath { get; set; }

        public string DataType { get; set; }

        public string CatalogId { get; set; }
    }
}
