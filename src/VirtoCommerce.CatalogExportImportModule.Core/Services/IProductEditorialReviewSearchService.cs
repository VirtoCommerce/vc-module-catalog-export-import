using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IProductEditorialReviewSearchService
    {
        Task<ProductEditorialReviewSearchResult> SearchEditorialReviewsAsync(ExportProductSearchCriteria criteria);
    }
}
