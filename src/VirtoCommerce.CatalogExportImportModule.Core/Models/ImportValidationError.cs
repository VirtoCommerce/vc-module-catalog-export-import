namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public sealed class ImportValidationError: ImportError
    {
        public string ErrorMessage { get; set; }
    }
}
