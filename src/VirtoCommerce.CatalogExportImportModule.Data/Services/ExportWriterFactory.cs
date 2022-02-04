using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.AssetsModule.Core.Assets;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class ExportWriterFactory : IExportWriterFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;

        public ExportWriterFactory(IBlobStorageProvider blobStorageProvider)
        {
            _blobStorageProvider = blobStorageProvider;
        }

        public IExportWriter<TExportable> Create<TExportable>(string filepath, CsvConfiguration csvConfiguration) where TExportable : IExportable
        {
            return new ExportWriter<TExportable>(filepath, _blobStorageProvider, csvConfiguration);
        }
    }
}
