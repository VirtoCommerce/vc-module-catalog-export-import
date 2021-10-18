using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IImportPagedDataSourceFactory
    {
        IImportPagedDataSource<TImport> Create<TImport>(string filePath, int pageSize, CsvConfiguration configuration = null) where TImport : IImportable;
    }
}
