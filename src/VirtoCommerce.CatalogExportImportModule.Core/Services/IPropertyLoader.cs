using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IPropertyLoader
    {
        Task<Property[]> LoadPropertiesAsync(LoadPropertiesCriteria criteria);
    }
}
