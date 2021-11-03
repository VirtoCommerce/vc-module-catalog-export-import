using System.Linq;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogExportImportModule.Data.ExportImport;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class ImportProductsClassMapFactory : IImportProductsClassMapFactory
    {
        private readonly IPropertyLoader _propertyLoader;
        private readonly IPropertyDictionaryItemSearchService _propertyDictionaryItemSearchService;

        public ImportProductsClassMapFactory(IPropertyLoader propertyLoader, IPropertyDictionaryItemSearchService propertyDictionaryItemSearchService)
        {
            _propertyLoader = propertyLoader;
            _propertyDictionaryItemSearchService = propertyDictionaryItemSearchService;
        }

        public async Task<ClassMap<CsvPhysicalProduct>> CreateClassMapAsync(string catalogId)
        {
            var properties = await
                _propertyLoader.LoadPropertiesAsync(new LoadPropertiesCriteria() { CatalogId = catalogId });


            var dictionaryPropsIds = properties.Where(x => x.Dictionary).Select(x => x.Id).ToArray();

            var dictionaryItemsSearchResult =
                await _propertyDictionaryItemSearchService.SearchAsync(
                    new PropertyDictionaryItemSearchCriteria() { PropertyIds = dictionaryPropsIds, Take = int.MaxValue });

            var propertyDictionaryItems = dictionaryItemsSearchResult.Results.GroupBy(x => x.PropertyId).ToDictionary(x => x.Key, x => x.ToArray());

            var classMap = new GenericTypeWithPropertiesClassMap<CsvPhysicalProduct>(properties, propertyDictionaryItems);

            return classMap;
        }
    }
}
