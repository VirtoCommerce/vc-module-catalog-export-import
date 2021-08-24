using System.Threading.Tasks;
using CsvHelper.Configuration;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface IImportPagedDataSourceFactory
    {
        Task<IImportPagedDataSource<TImport>> CreateAsync<TImport, TDomain>(string filePath, int pageSize, Configuration configuration = null) where TImport : IImportable;
    }
}
