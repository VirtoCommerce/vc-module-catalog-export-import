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
        private readonly IItemService _itemService;

        public PhysicalProductDataExporter(IExportPagedDataSourceFactory exportPagedDataSourceFactory, IExportWriterFactory exportWriterFactory, IOptions<PlatformOptions> platformOptions, IBlobStorageProvider blobStorageProvider, IBlobUrlResolver blobUrlResolver
                                            , ICategoryService categoryService, IItemService itemService)
            : base(exportPagedDataSourceFactory, exportWriterFactory, platformOptions, blobStorageProvider, blobUrlResolver)
        {
            _categoryService = categoryService;
            _itemService = itemService;
        }

        protected override async Task<ClassMap<CsvPhysicalProduct>> GetClassMapAsync(ExportDataRequest request)
        {
            // Get categories properties by request
            var properties = await LoadPropertiesAsync(request.ItemIds, request.CategoryIds);

            var classMap = new GenericTypeWithPropertiesClassMap<CsvPhysicalProduct>(properties);

            return classMap;
        }

        private async Task<Property[]> LoadPropertiesAsync(string[] itemIds, string[] categoryIds)
        {
            var result = Array.Empty<Property>();

            var comparer = AnonymousComparer.Create((Property x) => x.Id);

            if (!categoryIds.IsNullOrEmpty())
            {
                var categories =
                    (await _categoryService.GetByIdsAsync(categoryIds, CategoryResponseGroup.WithProperties.ToString()))
                    .ToArray();

                if (!categories.IsNullOrEmpty())
                {
                    var categoriesProperties = categories.SelectMany(x => x.Properties)
                        .Where(x => x.Type == PropertyType.Product)
                        .Distinct(comparer).ToArray();

                    result = categoriesProperties;
                }
            }

            if (!itemIds.IsNullOrEmpty())
            {
                var products =
                    (await _itemService.GetByIdsAsync(itemIds, ItemResponseGroup.WithProperties.ToString()))
                    .ToArray();

                if (!products.IsNullOrEmpty())
                {
                    var productsProperties = products.SelectMany(x => x.Properties)
                        .Where(x => x.Type == PropertyType.Product)
                        .Distinct(comparer).ToArray();

                    result = result.Union(productsProperties, comparer).ToArray();
                }
            }

            return result;
        }

        public override string DataType => ModuleConstants.DataTypes.PhysicalProduct;
    }
}
