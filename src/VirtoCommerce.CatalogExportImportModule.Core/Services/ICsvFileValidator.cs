using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface ICsvFileValidator
    {
        Task<ImportDataValidationResult> ValidateAsync(string dataType, string filePath);
    }
}
