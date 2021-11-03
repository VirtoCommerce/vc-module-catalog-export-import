using System.Threading.Tasks;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogExportImportModule.Data.ExportImport;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class PhysicalProductDataExporter : DataExporter<CsvPhysicalProduct>
    {
        private readonly IPropertyLoader _propertyLoader;

        public PhysicalProductDataExporter(IExportPagedDataSourceFactory exportPagedDataSourceFactory, IExportWriterFactory exportWriterFactory, IOptions<PlatformOptions> platformOptions, IBlobStorageProvider blobStorageProvider, IBlobUrlResolver blobUrlResolver
                                            , IPropertyLoader propertyLoader)
            : base(exportPagedDataSourceFactory, exportWriterFactory, platformOptions, blobStorageProvider, blobUrlResolver)
        {
            _propertyLoader = propertyLoader;
        }

        protected override async Task<ClassMap<CsvPhysicalProduct>> GetClassMapAsync(ExportDataRequest request)
        {
            // Get categories properties by request
            var properties = await _propertyLoader.LoadPropertiesAsync(new LoadPropertiesCriteria()
            {
                CatalogId = request.CatalogId,
                ItemIds = request.ItemIds,
                CategoryIds = request.CategoryIds
            });

            var classMap = new GenericTypeWithPropertiesClassMap<CsvPhysicalProduct>(properties);

            return classMap;
        }

        public override string DataType => ModuleConstants.DataTypes.PhysicalProduct;
    }
}
