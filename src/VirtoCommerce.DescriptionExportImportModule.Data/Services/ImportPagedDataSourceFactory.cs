using CsvHelper.Configuration;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public sealed class ImportPagedDataSourceFactory : IImportPagedDataSourceFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;


        public ImportPagedDataSourceFactory(IBlobStorageProvider blobStorageProvider)
        {
            _blobStorageProvider = blobStorageProvider;
        }

        public IImportPagedDataSource<TImportable> Create<TImportable>(string filePath, int pageSize, Configuration configuration = null) where TImportable : IImportable
        {
            configuration ??= new ImportConfiguration();
            return new ImportPagedDataSource<TImportable>(filePath, _blobStorageProvider, pageSize, configuration);
        }
    }
}
