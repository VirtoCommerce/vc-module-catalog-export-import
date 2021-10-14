using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class ImportProductSearchService : IImportProductSearchService
    {
        private const string PhysicalProductType = "Physical";

        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly IItemService _itemService;

        public ImportProductSearchService(Func<ICatalogRepository> catalogRepositoryFactory, IItemService itemService)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _itemService = itemService;
        }

        public async Task<ProductSearchResult> SearchAsync(ImportSearchCriteria criteria)
        {
            var result = new ProductSearchResult();

            using var catalogRepository = _catalogRepositoryFactory();

            // Optimize performance and CPU usage
            catalogRepository.DisableChangesTracking();

            var query = catalogRepository.Items;

            query = query.Where(x => x.ProductType == PhysicalProductType);


            if (!criteria.ObjectIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.ObjectIds.Contains(x.Id));
            }
            else if (!criteria.OuterIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.OuterIds.Contains(x.OuterId));
            }

            result.TotalCount = await query.CountAsync();

            var sortInfos = BuildSortExpression(criteria);

            if (criteria.Take > 0 && result.TotalCount > 0)
            {
                var ids = await query
                    .OrderBySortInfos(sortInfos)
                    .Select(x => x.Id)
                    .Skip(criteria.Skip)
                    .Take(criteria.Take)
                    .AsNoTracking()
                    .ToArrayAsync();

                result.Results = await _itemService.GetByIdsAsync(ids, ItemResponseGroup.ItemInfo.ToString());
            }

            return result;
        }


        protected virtual IList<SortInfo> BuildSortExpression(ImportSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo { SortColumn = nameof(ItemEntity.Id) }
                };
            }

            return sortInfos;
        }
    }
}
