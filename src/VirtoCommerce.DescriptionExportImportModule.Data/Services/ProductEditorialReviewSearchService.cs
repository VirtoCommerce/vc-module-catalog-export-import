using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class ProductEditorialReviewSearchService : IProductEditorialReviewSearchService
    {
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly IProductEditorialReviewService _productEditorialReviewService;
        private readonly IListEntrySearchService _listEntrySearchService;

        public ProductEditorialReviewSearchService(Func<ICatalogRepository> catalogRepositoryFactory, IProductEditorialReviewService productEditorialReviewService,
            IListEntrySearchService listEntrySearchService)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _productEditorialReviewService = productEditorialReviewService;
            _listEntrySearchService = listEntrySearchService;
        }

        public async Task<ProductEditorialReviewSearchResult> SearchEditorialReviewsAsync(ProductEditorialReviewSearchCriteria criteria, bool deepSearch = false)
        {
            if (deepSearch)
            {
                await ExtendSearchCriteriaForDeepSearch(criteria);
            }

            var result = new ProductEditorialReviewSearchResult();

            using var catalogRepository = _catalogRepositoryFactory();

            // Optimize performance and CPU usage
            catalogRepository.DisableChangesTracking();

            var descriptionQuery = catalogRepository.EditorialReviews;

            descriptionQuery = descriptionQuery.Where(x => x.CatalogItem.CatalogId == criteria.CatalogId);

            if (!criteria.CategoryIds.IsNullOrEmpty() && !criteria.ItemIds.IsNullOrEmpty())
            {
                descriptionQuery = descriptionQuery.Where(x => criteria.CategoryIds.Contains(x.CatalogItem.CategoryId)
                || criteria.ItemIds.Contains(x.ItemId));
            }
            else if (!criteria.CategoryIds.IsNullOrEmpty())
            {
                descriptionQuery = descriptionQuery.Where(x => criteria.CategoryIds.Contains(x.CatalogItem.CategoryId));
            }
            else if (!criteria.ItemIds.IsNullOrEmpty())
            {
                descriptionQuery = descriptionQuery.Where(x => criteria.ItemIds.Contains(x.ItemId));
            }

            result.TotalCount = await descriptionQuery.CountAsync();

            var sortInfos = BuildSortExpression(criteria);

            if (criteria.Take > 0 && result.TotalCount > 0)
            {
                var ids = await descriptionQuery
                    .OrderBySortInfos(sortInfos)
                    .Select(x => x.Id)
                    .Skip(criteria.Skip)
                    .Take(criteria.Take)
                    .AsNoTracking()
                    .ToArrayAsync();

                result.Results = await _productEditorialReviewService.GetByIdsAsync(ids);
            }

            return result;
        }

        protected virtual IList<SortInfo> BuildSortExpression(ProductEditorialReviewSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo { SortColumn = nameof(EditorialReviewEntity.Id) }
                };
            }

            return sortInfos;
        }


        /// <summary>
        /// Extend categories criteria with children categories. For the deep searching.
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        private async Task ExtendSearchCriteriaForDeepSearch(ProductEditorialReviewSearchCriteria searchCriteria)
        {
            // All from catalog
            if (string.IsNullOrEmpty(searchCriteria.Keyword) && searchCriteria.CategoryIds.IsNullOrEmpty() && searchCriteria.ItemIds.IsNullOrEmpty())
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

            // All with search by keyword 
            if (!string.IsNullOrEmpty(searchCriteria.Keyword) && searchCriteria.CategoryIds.IsNullOrEmpty() && searchCriteria.ItemIds.IsNullOrEmpty())
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
                }
            }

            // Extend CategoryIds with children. In case with searching by keyword this will work also. 
            if (!searchCriteria.CategoryIds.IsNullOrEmpty())
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
