using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IProductEditorialReviewService
    {
        Task<EditorialReview[]> GetByIdsAsync(string[] ids);
    }
}
