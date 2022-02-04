using CsvHelper.Configuration;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IImportConfigurationFactory
    {
        CsvConfiguration Create();
    }
}
