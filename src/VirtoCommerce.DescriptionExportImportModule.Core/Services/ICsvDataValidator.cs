using System.Threading.Tasks;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface ICsvDataValidator
    {
        Task<ImportDataValidationResult> ValidateAsync(string dataType, string filePath);
        Task<ImportDataValidationResult> ValidateAsync<T>(string filePath);
    }
}
