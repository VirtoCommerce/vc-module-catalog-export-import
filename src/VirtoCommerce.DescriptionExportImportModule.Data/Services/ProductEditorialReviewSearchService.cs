using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public ProductEditorialReviewSearchService(Func<ICatalogRepository> catalogRepositoryFactory, IProductEditorialReviewService productEditorialReviewService)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _productEditorialReviewService = productEditorialReviewService;
        }

        public async Task<ProductEditorialReviewSearchResult> SearchEditorialReviewsAsync(ProductEditorialReviewSearchCriteria criteria)
        {
            var result = new ProductEditorialReviewSearchResult();

            using var catalogRepository = _catalogRepositoryFactory();

            // Optimize performance and CPU usage
            catalogRepository.DisableChangesTracking();

            var descriptionQuery = catalogRepository.EditorialReviews;

            descriptionQuery = descriptionQuery.Where(x => x.CatalogItem.CatalogId == criteria.CatalogId);

            //var predicate = PredicateBuilder.True<EditorialReviewEntity>();

            //predicate = predicate.And(x => x.CatalogItem.CatalogId == criteria.CatalogId);

            //var predicate2 = PredicateBuilder.False<EditorialReviewEntity>();



            if (criteria.CategoryIds.Length > 0)
            {
                descriptionQuery = descriptionQuery.Where(x => criteria.CategoryIds.Contains(x.CatalogItem.CategoryId));
            }

            if (criteria.ItemIds.Length > 0)
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
    }
}
