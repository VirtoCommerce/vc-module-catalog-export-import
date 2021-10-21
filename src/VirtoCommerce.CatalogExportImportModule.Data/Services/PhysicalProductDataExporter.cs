using System;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogExportImportModule.Data.ExportImport;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class PhysicalProductDataExporter : DataExporter<CsvPhysicalProduct>
    {
        public PhysicalProductDataExporter(IExportPagedDataSourceFactory exportPagedDataSourceFactory, IExportWriterFactory exportWriterFactory, IOptions<PlatformOptions> platformOptions, IBlobStorageProvider blobStorageProvider, IBlobUrlResolver blobUrlResolver) : base(exportPagedDataSourceFactory, exportWriterFactory, platformOptions, blobStorageProvider, blobUrlResolver)
        {
            var classMap = new GenericTypeWithPropertiesClassMap<CsvPhysicalProduct>(Array.Empty<Property>());

            ClassMaps.Add(classMap);
        }

        public override string DataType => ModuleConstants.DataTypes.PhysicalProduct;
    }
}
