using System.Threading.Tasks;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface ICsvImportReporterFactory
    {
        Task<ICsvImportReporter> CreateAsync(string reportFilePath, string delimiter);
    }
}
