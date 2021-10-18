using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IExportWriterFactory
    {
        IExportWriter<TExportable> Create<TExportable>(string filepath, CsvConfiguration csvConfiguration) where TExportable : IExportable;
    }
}
