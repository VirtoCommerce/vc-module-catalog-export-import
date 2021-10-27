namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public sealed class ImportError
    {
        public string Error { get; set; }

        public string RawRow { get; set; }

        public int Row { get; set; }
    }
}
