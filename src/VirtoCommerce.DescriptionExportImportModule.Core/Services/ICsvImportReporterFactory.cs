using System.Threading.Tasks;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface ICsvImportReporterFactory
    {
        Task<ICsvImportReporter> CreateAsync(string reportFilePath, string delimiter);
    }
}
