using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IListEntryIndexedSearchService
    {
        Task<ListEntrySearchResult> SearchAsync(CatalogListEntrySearchCriteria criteria);
    }
}
