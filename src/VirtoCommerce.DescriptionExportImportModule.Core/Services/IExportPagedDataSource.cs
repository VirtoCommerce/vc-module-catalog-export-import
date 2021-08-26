using System.Threading.Tasks;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
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
