using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Caching;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class ProductEditorialReviewService : IProductEditorialReviewService
    {
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly IPlatformMemoryCache _platformMemoryCache;

        public ProductEditorialReviewService(
            Func<ICatalogRepository> catalogRepositoryFactory,
            IPlatformMemoryCache platformMemoryCache
            )
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _platformMemoryCache = platformMemoryCache;
        }

        public async Task<EditorialReview[]> GetByIdsAsync(string[] ids)
        {
            if (ids.IsNullOrEmpty())
            {
                return Array.Empty<EditorialReview>();
            }

            var cacheKey = CacheKey.With(GetType(), nameof(GetByIdsAsync), string.Join("-", ids.OrderBy(x => x)));

            var result = await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                using var catalogRepository = _catalogRepositoryFactory();

                var descriptions = await catalogRepository
                    .EditorialReviews
                    .Where(x => ids.Contains(x.Id))
                    .ToArrayAsync();

                var editorialReviews = descriptions
                    .Select(FillEntityProperties)
                    .ToArray();

                cacheEntry
                    .AddExpirationToken(
                        ItemCacheRegion.CreateChangeToken(
                            editorialReviews
                                .OfType<ExtendedEditorialReview>()
                                .Select(x => x.ItemId)
                                .Distinct()
                                .ToArray()
                        )
                    );

                return editorialReviews;
            });

            return result;
        }

        protected virtual EditorialReview FillEntityProperties(EditorialReviewEntity editorialReviewEntity)
        {
            var result = editorialReviewEntity.ToModel(AbstractTypeFactory<EditorialReview>.TryCreateInstance());

            if (result is ExtendedEditorialReview extendedEditorialReview)
            {
                extendedEditorialReview.ItemId = editorialReviewEntity.ItemId;
            }

            return result;
        }
    }
}
