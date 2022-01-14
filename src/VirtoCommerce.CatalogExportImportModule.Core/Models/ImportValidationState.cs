namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public sealed class ImportValidationState<T>
    {
        public ImportRecord<T> InvalidRecord { get; set; }
    }
}
