using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IExportPagedDataSource
    {
        int CurrentPageNumber { get; }
        int PageSize { get; }

        Task<int> GetTotalCountAsync();

        Task<bool> FetchAsync();

        IExportable[] Items { get; }
    }
}
