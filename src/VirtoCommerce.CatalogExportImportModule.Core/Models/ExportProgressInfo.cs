namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public class ExportProgressInfo
    {
        public int ProcessedCount { get; set; }

        public string Description { get; set; }

        public int TotalCount { get; set; }

        public string FileUrl { get; set; }
    }
}
