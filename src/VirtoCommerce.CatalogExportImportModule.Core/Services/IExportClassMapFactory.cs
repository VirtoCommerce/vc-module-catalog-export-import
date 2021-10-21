using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IExportClassMapFactory<TExportable> where TExportable : IExportable
    {
        public string DataType { get; }
        ClassMap<TExportable> Create();
    }
}
