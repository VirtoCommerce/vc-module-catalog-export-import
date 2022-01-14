namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface ICsvParsingErrorHandlerFactory
    {
        public ICsvParsingErrorHandler Create(ICsvImportErrorReporter importErrorReporter);
    }
}
