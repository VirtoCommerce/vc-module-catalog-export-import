using CsvHelper.Configuration;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface IImportPagedDataSourceFactory
    {
        IImportPagedDataSource<TImport> Create<TImport>(string filePath, int pageSize, Configuration configuration = null) where TImport : IImportable;
    }
}
