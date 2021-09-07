using System.Threading.Tasks;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface IProductEditorialReviewSearchService
    {
        Task<ProductEditorialReviewSearchResult> SearchEditorialReviewsAsync(ProductEditorialReviewSearchCriteria criteria);
    }
}
