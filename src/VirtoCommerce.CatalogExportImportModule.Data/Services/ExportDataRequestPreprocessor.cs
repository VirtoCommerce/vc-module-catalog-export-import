using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using catalogCore = VirtoCommerce.CatalogModule.Core;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public sealed class ExportDataRequestPreprocessor : IExportDataRequestPreprocessor
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IListEntryIndexedSearchService _listEntrySearchService;

        public ExportDataRequestPreprocessor(ISettingsManager settingsManager, IListEntryIndexedSearchService listEntrySearchService)
        {
            _settingsManager = settingsManager;
            _listEntrySearchService = listEntrySearchService;
        }

        public async Task PreprocessRequestAsync(ExportDataRequest dataRequest, bool extendCategoriesWithChildren = false)
        {
            var useIndexedSearch = _settingsManager.GetValue(catalogCore.ModuleConstants.Settings.Search.UseCatalogIndexedSearchInManager.Name, true);

            if (!dataRequest.ItemIds.IsNullOrEmpty() && !dataRequest.CategoryIds.IsNullOrEmpty())
            {
                dataRequest.Keyword = null;
            }

            // All with searching by keyword from catalog or category
            if (!string.IsNullOrEmpty(dataRequest.Keyword))
            {
                var listEntrySearchCriteria = new CatalogListEntrySearchCriteria()
                {
                    CatalogId = dataRequest.CatalogId,
                    CategoryId = dataRequest.CategoryId,
                    Keyword = dataRequest.Keyword,
                    SearchInChildren = true, // at index searching it search in children always. in this case flag means nothing. 
                    SearchInVariations = true, // at index searching it search in variations always. in this case flag means nothing.
                    Take = useIndexedSearch ? ModuleConstants.ElasticMaxTake : int.MaxValue // workaround for ElasticSearch
                };

                var listEntrySearchResult = await _listEntrySearchService.SearchAsync(listEntrySearchCriteria);

                if (listEntrySearchResult.TotalCount > 0)
                {
                    var listEntries = listEntrySearchResult.Results;
                    var categoriesIds = listEntries.Where(x => x.Type.EqualsInvariant("category")).Select(x => x.Id).ToArray();
                    var productsIds = listEntries.Where(x => x.Type.EqualsInvariant("product")).Select(x => x.Id).ToArray();
                    dataRequest.CategoryIds = categoriesIds;
                    dataRequest.ItemIds = productsIds;
                }
            }

            // Set current as selected if there is not selected or founded cats and products
            if (!string.IsNullOrEmpty(dataRequest.CategoryId) && dataRequest.CategoryIds.IsNullOrEmpty() && dataRequest.ItemIds.IsNullOrEmpty())
            {
                dataRequest.CategoryIds = new[] { dataRequest.CategoryId };
            }

            // Extend CategoryIds with children. In case with searching by keyword this will work also. 
            if (extendCategoriesWithChildren && !dataRequest.CategoryIds.IsNullOrEmpty())
            {
                var listEntrySearchCriteria = new CatalogListEntrySearchCriteria()
                {
                    CatalogId = dataRequest.CatalogId,
                    ObjectType = nameof(Category),
                    CategoryIds = dataRequest.CategoryIds,
                    SearchInChildren = true,
                    SearchInVariations = true,
                    Take = useIndexedSearch ? ModuleConstants.ElasticMaxTake : int.MaxValue // workaround for ElasticSearch
                };

                var listEntrySearchResult = await _listEntrySearchService.SearchAsync(listEntrySearchCriteria);

                if (listEntrySearchResult.TotalCount > 0)
                {
                    var categoryIds = listEntrySearchResult.Results.Select(x => x.Id).ToArray();
                    dataRequest.CategoryIds = dataRequest.CategoryIds.Union(categoryIds, StringComparer.InvariantCulture).ToArray();
                }
            }
        }
    }
}
