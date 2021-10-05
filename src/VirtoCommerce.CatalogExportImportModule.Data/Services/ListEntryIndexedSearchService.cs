using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.ListEntry;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    /// <summary>
    /// Service for indexed searching of list entries.
    /// Logic moved from
    /// https://github.com/VirtoCommerce/vc-module-catalog/blob/a82b365207d280d25b06b76e933bffd23369fe68/src/VirtoCommerce.CatalogModule.Web/Controllers/Api/CatalogModuleListEntryController.cs#L300
    /// it should be excluded when next bug will be eliminated https://virtocommerce.atlassian.net/browse/PT-2906. 
    /// </summary>
    public sealed class ListEntryIndexedSearchService : IListEntryIndexedSearchService
    {
        private readonly IProductIndexedSearchService _productIndexedSearchService;
        private readonly ICategoryIndexedSearchService _categoryIndexedSearchService;
        private readonly IListEntrySearchService _listEntrySearchService;
        private readonly ISettingsManager _settingsManager;

        public ListEntryIndexedSearchService(
            IProductIndexedSearchService productIndexedSearchService,
            ICategoryIndexedSearchService categoryIndexedSearchService,
            IListEntrySearchService listEntrySearchService,
            ISettingsManager settingsManager
            )
        {
            _productIndexedSearchService = productIndexedSearchService;
            _categoryIndexedSearchService = categoryIndexedSearchService;
            _listEntrySearchService = listEntrySearchService;
            _settingsManager = settingsManager;
        }
        public async Task<ListEntrySearchResult> SearchAsync(CatalogListEntrySearchCriteria criteria)
        {
            var result = new ListEntrySearchResult();
            var useIndexedSearch = _settingsManager.GetValue(ModuleConstants.Settings.Search.UseCatalogIndexedSearchInManager.Name, true);

            if (useIndexedSearch && !string.IsNullOrEmpty(criteria.Keyword))
            {
                var categoryIndexedSearchCriteria = AbstractTypeFactory<CategoryIndexedSearchCriteria>.TryCreateInstance().FromListEntryCriteria(criteria) as CategoryIndexedSearchCriteria;
                const CategoryResponseGroup catResponseGroup = CategoryResponseGroup.Info | CategoryResponseGroup.WithOutlines;
                categoryIndexedSearchCriteria.ResponseGroup = catResponseGroup.ToString();

                var catIndexedSearchResult = await _categoryIndexedSearchService.SearchAsync(categoryIndexedSearchCriteria);
                var totalCount = catIndexedSearchResult.TotalCount;
                var skip = Math.Min(totalCount, criteria.Skip);
                var take = Math.Min(criteria.Take, Math.Max(0, totalCount - criteria.Skip));

                result.Results = catIndexedSearchResult.Items.Select(x => AbstractTypeFactory<CategoryListEntry>.TryCreateInstance().FromModel(x)).ToList();
                result.TotalCount = (int)totalCount;

                criteria.Skip -= (int)skip;
                criteria.Take -= (int)take;

                const ItemResponseGroup itemResponseGroup = ItemResponseGroup.ItemInfo | ItemResponseGroup.Outlines;

                var productIndexedSearchCriteria = AbstractTypeFactory<ProductIndexedSearchCriteria>.TryCreateInstance().FromListEntryCriteria(criteria) as ProductIndexedSearchCriteria;
                productIndexedSearchCriteria.ResponseGroup = itemResponseGroup.ToString();

                var indexedSearchResult = await _productIndexedSearchService.SearchAsync(productIndexedSearchCriteria);
                result.TotalCount += (int)indexedSearchResult.TotalCount;
                result.Results.AddRange(indexedSearchResult.Items.Select(x => AbstractTypeFactory<ProductListEntry>.TryCreateInstance().FromModel(x)));
            }
            else
            {
                result = await _listEntrySearchService.SearchAsync(criteria);
            }

            return result;
        }
    }
}
