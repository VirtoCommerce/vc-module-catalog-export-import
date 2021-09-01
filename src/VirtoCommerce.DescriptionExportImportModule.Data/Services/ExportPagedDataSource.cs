using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class ExportPagedDataSource : IExportPagedDataSource

    {
        private readonly IProductEditorialReviewSearchService _productEditorialReviewSearchService;
        private readonly IItemService _itemService;
        private readonly IListEntrySearchService _listEntrySearchService;
        private readonly ExportDataRequest _exportRequest;

        public int CurrentPageNumber { get; private set; }

        public int PageSize { get; }

        public IExportable[] Items { get; private set; }

        public ExportPagedDataSource(
            IProductEditorialReviewSearchService productEditorialReviewSearchService,
            IItemService itemService,
            IListEntrySearchService listEntrySearchService,
            int pageSize,
            ExportDataRequest exportRequest
            )
        {
            _productEditorialReviewSearchService = productEditorialReviewSearchService;
            _itemService = itemService;
            _listEntrySearchService = listEntrySearchService;

            _exportRequest = exportRequest;
            PageSize = pageSize;
        }

        public async Task<int> GetTotalCountAsync()
        {
            var searchCriteria = _exportRequest.ToSearchCriteria();
            searchCriteria.Take = 0;

            var searchResult = await _productEditorialReviewSearchService.SearchEditorialReviewsAsync(searchCriteria);

            return searchResult.TotalCount;
        }

        public async Task<bool> FetchAsync()
        {
            if (CurrentPageNumber * PageSize >= await GetTotalCountAsync())
            {
                Items = Array.Empty<IExportable>();
                return false;
            }

            var searchCriteria = _exportRequest.ToSearchCriteria();


            await HandleSearchCriteriaAsync(searchCriteria);

            searchCriteria.Skip = CurrentPageNumber * PageSize;
            searchCriteria.Take = PageSize;

            var searchResult = await _productEditorialReviewSearchService.SearchEditorialReviewsAsync(searchCriteria);

            Items = searchResult
                .Results
                .OfType<ExtendedEditorialReview>()
                .Select(async x =>
                {
                    var product = await _itemService.GetByIdAsync(x.ItemId, ItemResponseGroup.ItemInfo.ToString());
                    var result = new CsvEditorialReview
                    {
                        ProductName = product.Name,
                        ProductSku = product.Code,
                        DescriptionId = x.Id,
                        DescriptionContent = x.Content,
                        Language = x.LanguageCode,
                        Type = x.ReviewType,
                    };

                    return result;
                })
                .Select(x => x.Result)
                .ToArray<IExportable>();

            CurrentPageNumber++;

            return true;
        }

        private async Task HandleSearchCriteriaAsync(ProductEditorialReviewSearchCriteria searchCriteria)
        {
            // All with search by keyword 
            if (!string.IsNullOrEmpty(searchCriteria.Keyword) && searchCriteria.CategoryIds.Length == 0 && searchCriteria.ItemIds.Length == 0)
            {
                var listEntrySearchCriteria = new CatalogListEntrySearchCriteria()
                {
                    CatalogId = searchCriteria.CatalogId,
                    Keyword = searchCriteria.Keyword,
                    SearchInChildren = true,
                    SearchInVariations = true,
                    Take = int.MaxValue
                };

                var listEntrySearchResult = await _listEntrySearchService.SearchAsync(listEntrySearchCriteria);

                if (listEntrySearchResult.TotalCount > 0)
                {
                    var listEntries = listEntrySearchResult.Results;

                    var categoriesIds = listEntries.Where(x => x.Type.EqualsInvariant("category")).Select(x => x.Id).ToArray();

                    var productsIds = listEntries.Where(x => x.Type.EqualsInvariant("product")).Select(x => x.Id).ToArray();

                    searchCriteria.CategoryIds = categoriesIds;

                    searchCriteria.ItemIds = productsIds;

                    return;
                }
            }

            // All from catalog
            if (string.IsNullOrEmpty(searchCriteria.Keyword) && searchCriteria.CategoryIds.Length == 0 && searchCriteria.ItemIds.Length == 0)
            {
                var listEntrySearchCriteria = new CatalogListEntrySearchCriteria()
                {
                    CatalogId = searchCriteria.CatalogId,
                    SearchInChildren = true,
                    SearchInVariations = true,
                    Take = int.MaxValue
                };

                var listEntrySearchResult = await _listEntrySearchService.SearchAsync(listEntrySearchCriteria);

                if (listEntrySearchResult.TotalCount > 0)
                {
                    var listEntries = listEntrySearchResult.Results;

                    var categoriesIds = listEntries.Where(x => x.Type.EqualsInvariant("category")).Select(x => x.Id).ToArray();

                    var productsIds = listEntries.Where(x => x.Type.EqualsInvariant("product")).Select(x => x.Id).ToArray();

                    searchCriteria.CategoryIds = categoriesIds;

                    searchCriteria.ItemIds = productsIds;

                    return;
                }
            }

            // For selected cases. Extend CategoryIds with children
            if (searchCriteria.CategoryIds.Length > 0)
            {
                var listEntrySearchCriteria = new CatalogListEntrySearchCriteria()
                {
                    CatalogId = searchCriteria.CatalogId,
                    ObjectType = nameof(Category),
                    CategoryIds = searchCriteria.CategoryIds,
                    SearchInChildren = true,
                    SearchInVariations = true,
                    Take = int.MaxValue
                };

                var listEntrySearchResult = await _listEntrySearchService.SearchAsync(listEntrySearchCriteria);

                if (listEntrySearchResult.TotalCount > 0)
                {

                    var categoryIds = listEntrySearchResult.Results.Select(x => x.Id).ToArray();

                    searchCriteria.CategoryIds = searchCriteria.CategoryIds.Union(categoryIds).ToArray();
                }
            }
        }
    }
}
