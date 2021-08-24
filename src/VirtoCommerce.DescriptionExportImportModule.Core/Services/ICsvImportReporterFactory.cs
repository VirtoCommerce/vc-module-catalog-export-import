using System.Threading.Tasks;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface ICsvCustomerImportReporterFactory
    {
        Task<ICsvImportReporter> CreateAsync(string reportFilePath, string delimiter);
    }
}
