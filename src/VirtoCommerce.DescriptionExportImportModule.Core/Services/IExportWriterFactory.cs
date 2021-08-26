using CsvHelper.Configuration;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface IExportWriterFactory
    {
        IExportWriter Create(string filepath, Configuration csvConfiguration);
    }
}
