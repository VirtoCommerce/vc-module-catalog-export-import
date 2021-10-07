using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class ProductEditorialReviewSearchService : IProductEditorialReviewSearchService
    {

        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly IProductEditorialReviewService _productEditorialReviewService;
        private readonly IListEntryIndexedSearchService _listEntrySearchService;
        private readonly ISettingsManager _settingsManager;

        public ProductEditorialReviewSearchService(Func<ICatalogRepository> catalogRepositoryFactory, IProductEditorialReviewService productEditorialReviewService,
            IListEntryIndexedSearchService listEntrySearchService,
            ISettingsManager settingsManager
            )
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _productEditorialReviewService = productEditorialReviewService;
            _listEntrySearchService = listEntrySearchService;
            _settingsManager = settingsManager;
        }

        public async Task<ProductEditorialReviewSearchResult> SearchEditorialReviewsAsync(ExportProductSearchCriteria criteria)
        {
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

        protected virtual IList<SortInfo> BuildSortExpression(ExportProductSearchCriteria criteria)
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
    }
}
