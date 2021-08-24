using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface IDescriptionExportPagedDataSource
    {
        int CurrentPageNumber { get; }
        int PageSize { get; }
        Task<int> GetTotalCountAsync();
        Task<bool> FetchAsync();
        EditorialReview[] Items { get; }
    }
}
