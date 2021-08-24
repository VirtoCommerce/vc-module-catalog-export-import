using System.Threading.Tasks;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface IProductDescriptionSearchService
    {
        Task<ProductDescriptionSearchResult> SearchProductDescriptionsAsync(ProductDescriptionSearchCriteria criteria);
    }
}
