using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogModule.Core.Model.Search;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IExportProductSearchService
    {
        Task<ProductSearchResult> SearchAsync(ExportProductSearchCriteria criteria);
    }
}
