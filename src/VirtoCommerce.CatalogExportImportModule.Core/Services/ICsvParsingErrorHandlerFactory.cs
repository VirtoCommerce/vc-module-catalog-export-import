namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface ICsvParsingErrorHandlerFactory
    {
        ICsvParsingErrorHandler Create(ICsvImportErrorReporter importErrorReporter);
    }
}
