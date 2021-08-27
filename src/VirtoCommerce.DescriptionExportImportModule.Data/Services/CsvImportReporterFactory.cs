using System.Threading.Tasks;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public sealed class CsvImportReporterFactory : ICsvImportReporterFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        public CsvImportReporterFactory(IBlobStorageProvider blobStorageProvider)
        {
            _blobStorageProvider = blobStorageProvider;
        }

        public async Task<ICsvImportReporter> CreateAsync(string reportFilePath, string delimiter)
        {
            var reportBlob = await _blobStorageProvider.GetBlobInfoAsync(reportFilePath);

            if (reportBlob != null)
            {
                await _blobStorageProvider.RemoveAsync(new[] { reportFilePath });
            }

            return new CsvImportReporter(reportFilePath, _blobStorageProvider, delimiter);
        }
    }
}
