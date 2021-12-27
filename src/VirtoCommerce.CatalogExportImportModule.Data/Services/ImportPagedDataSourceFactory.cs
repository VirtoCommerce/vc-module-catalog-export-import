using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.AssetsModule.Core.Assets;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public sealed class ImportPagedDataSourceFactory : IImportPagedDataSourceFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly ImportConfigurationFactory _importConfigurationFactory;


        public ImportPagedDataSourceFactory(IBlobStorageProvider blobStorageProvider, ImportConfigurationFactory importConfigurationFactory)
        {
            _blobStorageProvider = blobStorageProvider;
            _importConfigurationFactory = importConfigurationFactory;
        }

        public IImportPagedDataSource<TImportable> Create<TImportable>(string filePath, int pageSize, CsvConfiguration configuration = null) where TImportable : IImportable
        {
            configuration ??= _importConfigurationFactory.Create();
            return new ImportPagedDataSource<TImportable>(filePath, _blobStorageProvider, pageSize, configuration);
        }
    }
}
