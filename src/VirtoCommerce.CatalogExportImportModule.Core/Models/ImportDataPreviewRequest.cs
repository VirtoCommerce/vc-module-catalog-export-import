namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public sealed class ImportDataPreviewRequest
    {
        public string CatalogId { get; set; }

        public string FilePath { get; set; }

        public string DataType { get; set; }
    }
}
