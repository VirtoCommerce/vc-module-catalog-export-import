using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class ExportWriterFactory : IExportWriterFactory
    {
        private readonly IBlobStorageProvider _blobStorageProvider;

        //private static Dictionary<string, Type> registeredTypes = new Dictionary<string, System.Type>();

        //static GenericFactory()
        //{
        //    registeredTypes.Add(ModuleConstants.DataTypes.EditorialReview, typeof(ExportWriter<CsvEditorialReview>));
        //    registeredTypes.Add(ModuleConstants.DataTypes.PhysicalProduct, typeof(GenericString));
        //}

        public ExportWriterFactory(IBlobStorageProvider blobStorageProvider)
        {
            _blobStorageProvider = blobStorageProvider;
        }

        public IExportWriter<TExportable> Create<TExportable>(string filepath, Configuration csvConfiguration) where TExportable : IExportable
        {
            return new ExportWriter<TExportable>(filepath, _blobStorageProvider, csvConfiguration);
        }
    }
}
