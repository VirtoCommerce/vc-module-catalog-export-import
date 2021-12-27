using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.AssetsModule.Core.Assets;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
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
