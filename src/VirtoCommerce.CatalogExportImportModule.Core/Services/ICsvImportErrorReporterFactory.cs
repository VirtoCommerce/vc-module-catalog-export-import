using System.Threading.Tasks;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface ICsvImportErrorReporterFactory
    {
        Task<ICsvImportErrorReporter> CreateAsync(string reportFilePath,string delimiter);
    }
}
