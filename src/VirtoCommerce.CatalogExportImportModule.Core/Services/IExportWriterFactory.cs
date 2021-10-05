using CsvHelper.Configuration;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IExportWriterFactory
    {
        IExportWriter Create(string filepath, Configuration csvConfiguration);
    }
}
