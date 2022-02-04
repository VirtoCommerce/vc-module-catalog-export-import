using CsvHelper.Configuration;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface ICsvParsingErrorHandler
    {
        void HandleErrors(CsvConfiguration configuration);
    }
}
