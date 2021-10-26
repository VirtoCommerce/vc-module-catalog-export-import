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
    public class ExportProductSearchService : IExportProductSearchService
    {
        private const string PhysicalProductType = "Physical";

        private readonly Func<ICatalogRepository> _catalogRepositoryFactory;
        private readonly IItemService _itemService;

        public ExportProductSearchService(Func<ICatalogRepository> catalogRepositoryFactory, IItemService itemService)
        {
            _catalogRepositoryFactory = catalogRepositoryFactory;
            _itemService = itemService;
        }

        public async Task<ProductSearchResult> SearchAsync(ExportProductSearchCriteria criteria)
        {
            var result = new ProductSearchResult();

            using var catalogRepository = _catalogRepositoryFactory();

            // Optimize performance and CPU usage
            catalogRepository.DisableChangesTracking();

            var query = catalogRepository.Items;

            query = query.Where(x => x.CatalogId == criteria.CatalogId && x.ParentId == null && x.ProductType == PhysicalProductType);


            if (!criteria.CategoryIds.IsNullOrEmpty() && !criteria.ItemIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CategoryIds.Contains(x.CategoryId)
                                                               || criteria.ItemIds.Contains(x.Id));
            }
            else if (!criteria.CategoryIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CategoryIds.Contains(x.CategoryId));
            }
            else if (!criteria.ItemIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.ItemIds.Contains(x.Id));
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

                result.Results = await _itemService.GetByIdsAsync(ids, respGroup: (ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemProperties).ToString());
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
                    new SortInfo { SortColumn = nameof(ItemEntity.Id) }
                };
            }

            return sortInfos;
        }
    }
}
