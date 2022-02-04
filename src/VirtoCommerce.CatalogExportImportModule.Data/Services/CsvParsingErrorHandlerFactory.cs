using VirtoCommerce.CatalogExportImportModule.Core.Services;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public sealed class CsvParsingErrorHandlerFactory: ICsvParsingErrorHandlerFactory
    {
        public ICsvParsingErrorHandler Create(ICsvImportErrorReporter importErrorReporter)
        {
            return new CsvParsingErrorHandler(importErrorReporter);
        }
    }
}
