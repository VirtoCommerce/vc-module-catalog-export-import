using System;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public sealed class ExtendedProductSearchService : ProductSearchService
    {
        public ExtendedProductSearchService(Func<ICatalogRepository> catalogRepositoryFactory, IItemService itemService) : base(catalogRepositoryFactory, itemService)
        {
        }

        protected override IQueryable<ItemEntity> BuildQuery(ICatalogRepository repository, ProductSearchCriteria criteria)
        {
            var query = base.BuildQuery(repository, criteria);

            if (!criteria.ObjectIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.ObjectIds.Contains(x.Id));
            }

            return query;
        }
    }
}
