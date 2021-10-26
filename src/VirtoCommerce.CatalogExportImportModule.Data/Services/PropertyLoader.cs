using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public sealed class PropertyLoader : IPropertyLoader
    {
        private readonly ICategoryService _categoryService;
        private readonly IItemService _itemService;
        private readonly IPropertyService _propertyService;

        public PropertyLoader(ICategoryService categoryService, IItemService itemService, IPropertyService propertyService)
        {
            _categoryService = categoryService;
            _itemService = itemService;
            _propertyService = propertyService;
        }
        public async Task<Property[]> LoadPropertiesAsync(LoadPropertiesCriteria criteria)
        {
            var result = Array.Empty<Property>();

            var comparer = AnonymousComparer.Create((Property x) => x.Name);

            if (criteria.CategoryIds.IsNullOrEmpty() && criteria.ItemIds.IsNullOrEmpty())
            {
                var allCatalogProperties = await _propertyService.GetAllCatalogPropertiesAsync(criteria.CatalogId);

                var allProductsProperties = allCatalogProperties.Where(x => x.Type == PropertyType.Product && x.IsManageable).ToArray();

                result = allProductsProperties;
            }

            if (!criteria.CategoryIds.IsNullOrEmpty())
            {
                var categories =
                    (await _categoryService.GetByIdsAsync(criteria.CategoryIds, CategoryResponseGroup.WithProperties.ToString()))
                    .ToArray();

                if (!categories.IsNullOrEmpty())
                {
                    var categoriesProperties = categories.SelectMany(x => x.Properties)
                        .Where(x => x.Type == PropertyType.Product && x.IsManageable)
                        .Distinct(comparer).ToArray();

                    result = categoriesProperties;
                }
            }

            if (!criteria.ItemIds.IsNullOrEmpty())
            {
                var products =
                    (await _itemService.GetByIdsAsync(criteria.ItemIds, ItemResponseGroup.WithProperties.ToString()))
                    .ToArray();

                if (!products.IsNullOrEmpty())
                {
                    var productsProperties = products.SelectMany(x => x.Properties)
                        .Where(x => x.Type == PropertyType.Product && x.IsManageable)
                        .Distinct(comparer).ToArray();

                    result = result.Union(productsProperties, comparer).ToArray();
                }
            }

            return result;
        }
    }
}
