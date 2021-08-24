using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public ProductDescriptionSearchService(Func<ICatalogRepository> catalogRepositoryFactory)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
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

            if (criteria.Take > 0 && result.TotalCount > 0)
            {
                var ids = await descriptionQuery
                    .OrderBySortInfos(criteria.SortInfos)
                    .Select(x => x.Id)
                    .Skip(criteria.Skip)
                    .Take(criteria.Take)
                    .AsNoTracking()
                    .ToArrayAsync();

                //TODO: select descrs
            }

            return result;
        }
    }
}
