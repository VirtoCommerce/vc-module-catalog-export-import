using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public sealed class ExportProductSearchService : IExportProductSearchService
    {
        private const string PhysicalProductType = "Physical";

        private readonly IProductSearchService _productSearchService;

        public ExportProductSearchService(IProductSearchService productSearchService)
        {
            _productSearchService = productSearchService;
        }

        public async Task<ProductSearchResult> SearchAsync(ExportProductSearchCriteria criteria)
        {
            var result = new ProductSearchResult();

            if (!criteria.CategoryIds.IsNullOrEmpty())
            {
                var resultByCategories = await _productSearchService.SearchProductsAsync(new ProductSearchCriteria()
                {
                    ProductTypes = new[] { PhysicalProductType },
                    CatalogId = criteria.CatalogId,
                    CategoryIds = criteria.CategoryIds,
                    SearchInChildren = true,
                    SearchInVariations = false,
                    Take = criteria.Take,
                    Skip = criteria.Skip,
                });

                result.Results = resultByCategories.Results;
                result.TotalCount = resultByCategories.TotalCount;
            }

            var totalCount = result.TotalCount;
            var skip = Math.Min(totalCount, criteria.Skip);
            var take = Math.Min(criteria.Take, Math.Max(0, totalCount - criteria.Skip));

            var itemIds = criteria.ItemIds;

            var foundedItemIds = result.Results.Select(x => x.Id).ToArray();

            itemIds = itemIds.Except(foundedItemIds).ToArray();

            if (!itemIds.IsNullOrEmpty())
            {
                var resultByItems = await _productSearchService.SearchProductsAsync(new ProductSearchCriteria()
                {
                    ProductTypes = new[] { PhysicalProductType },
                    CatalogId = criteria.CatalogId,
                    ObjectIds = criteria.ItemIds,
                    SearchInChildren = false,
                    SearchInVariations = false,
                    Take = take,
                    Skip = skip,
                });

                result.Results.AddRange(resultByItems.Results);
                result.TotalCount += resultByItems.TotalCount;
            }

            return result;
        }
    }
}
