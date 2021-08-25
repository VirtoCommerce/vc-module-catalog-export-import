using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface IProductDescriptionService
    {
        Task<EditorialReview[]> GetByIdsAsync(string[] ids);
    }
}
