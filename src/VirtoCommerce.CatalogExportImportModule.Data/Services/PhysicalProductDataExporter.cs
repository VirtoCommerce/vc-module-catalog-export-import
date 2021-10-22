using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogExportImportModule.Data.ExportImport;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class PhysicalProductDataExporter : DataExporter<CsvPhysicalProduct>
    {
        private readonly ICategoryService _categoryService;

        public PhysicalProductDataExporter(IExportPagedDataSourceFactory exportPagedDataSourceFactory, IExportWriterFactory exportWriterFactory, IOptions<PlatformOptions> platformOptions, IBlobStorageProvider blobStorageProvider, IBlobUrlResolver blobUrlResolver
                                            , ICategoryService categoryService)
            : base(exportPagedDataSourceFactory, exportWriterFactory, platformOptions, blobStorageProvider, blobUrlResolver)
        {
            _categoryService = categoryService;
        }

        protected override async Task<ClassMap<CsvPhysicalProduct>> GetClassMapAsync(ExportDataRequest request)
        {
            // Get categories properties by request
            var properties = await LoadPropertiesAsync(request.CategoryIds);

            var classMap = new GenericTypeWithPropertiesClassMap<CsvPhysicalProduct>(properties);

            return classMap;
        }

        private async Task<Property[]> LoadPropertiesAsync(string[] requestCategoryIds)
        {
            var categories = (await _categoryService.GetByIdsAsync(requestCategoryIds, CategoryResponseGroup.WithProperties.ToString())).ToArray();

            if (categories.IsNullOrEmpty())
            {
                return Array.Empty<Property>();
            }

            var properties = categories.SelectMany(x => x.Properties)
                .Where(x => x.Type == PropertyType.Product)
                .Distinct(AnonymousComparer.Create((Property x) => x.Id)).ToArray();

            return properties;
        }

        public override string DataType => ModuleConstants.DataTypes.PhysicalProduct;
    }
}
