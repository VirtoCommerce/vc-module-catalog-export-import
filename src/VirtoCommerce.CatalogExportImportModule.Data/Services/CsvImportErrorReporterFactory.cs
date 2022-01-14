using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.AssetsModule.Core.Assets;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public sealed class CsvImportErrorReporterFactory : ICsvImportErrorReporterFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        public CsvImportErrorReporterFactory(IBlobStorageProvider blobStorageProvider)
        {
            _blobStorageProvider = blobStorageProvider;
        }

        public async Task<ICsvImportErrorReporter> CreateAsync(string reportFilePath, string delimiter)
        {
            var reportBlob = await _blobStorageProvider.GetBlobInfoAsync(reportFilePath);

            if (reportBlob != null)
            {
                await _blobStorageProvider.RemoveAsync(new[] { reportFilePath });
            }

            return new CsvImportErrorReporter(_blobStorageProvider, reportFilePath, delimiter);
        }
    }
}
