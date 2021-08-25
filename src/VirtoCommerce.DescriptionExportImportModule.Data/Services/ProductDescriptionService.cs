using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class ProductDescriptionService : IProductDescriptionService
    {
        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;

        public ProductDescriptionService(Func<ICatalogRepository> catalogRepositoryFactory)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
        }

        public async Task<EditorialReview[]> GetByIdsAsync(string[] ids)
        {
            using var catalogRepository = _catalogRepositoryFactory();

            var descriptions = await catalogRepository
                .EditorialReviews
                .Where(x => ids.Contains(x.Id))
                .ToArrayAsync();

            var result = descriptions
                .Select(FillEntityProperties)
                .ToArray();

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
