using CsvHelper.Configuration;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class ExportWriterFactory : IExportWriterFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;

        public ExportWriterFactory(IBlobStorageProvider blobStorageProvider)
        {
            _blobStorageProvider = blobStorageProvider;
        }

        public IExportWriter Create(string filepath, CsvConfiguration csvConfiguration)
        {
            return new ExportWriter(filepath, _blobStorageProvider, csvConfiguration);
        }
    }
}
