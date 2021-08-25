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
    public class ProductDescriptionSearchService : IProductDescriptionSearchService
    {
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly IProductDescriptionService _productDescriptionService;

        public ProductDescriptionSearchService(Func<ICatalogRepository> catalogRepositoryFactory, IProductDescriptionService productDescriptionService)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _productDescriptionService = productDescriptionService;
        }

        public async Task<ProductDescriptionSearchResult> SearchProductDescriptionsAsync(ProductDescriptionSearchCriteria criteria)
        {
            var result = new ProductDescriptionSearchResult();

            using var catalogRepository = _catalogRepositoryFactory();

            // Optimize performance and CPU usage
            catalogRepository.DisableChangesTracking();

            var descriptionQuery = catalogRepository.EditorialReviews;

            descriptionQuery = descriptionQuery
                .Where(x => x.CatalogItem.CategoryId == criteria.CategoryId && x.CatalogItem.CatalogId == criteria.CatalogId);

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

                result.Results = await _productDescriptionService.GetByIdsAsync(ids);
            }

            return result;
        }

        protected virtual IList<SortInfo> BuildSortExpression(ProductDescriptionSearchCriteria criteria)
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
