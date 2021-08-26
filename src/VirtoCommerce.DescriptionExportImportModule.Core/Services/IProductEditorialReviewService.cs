using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface IProductEditorialReviewService
    {
        Task<EditorialReview[]> GetByIdsAsync(string[] ids);
    }
}
