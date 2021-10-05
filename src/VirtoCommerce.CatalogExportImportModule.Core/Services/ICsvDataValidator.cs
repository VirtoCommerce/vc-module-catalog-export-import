using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface ICsvDataValidator
    {
        Task<ImportDataValidationResult> ValidateAsync(string dataType, string filePath);
        Task<ImportDataValidationResult> ValidateAsync<T>(string filePath);
    }
}
