using System;
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
        private const string _physicalProductType = "Physical";

        private readonly IProductSearchService _productSearchService;
        public ExportProductSearchService(IProductSearchService productSearchService)
        {
            _productSearchService = productSearchService;
        }

        public async Task<ProductSearchResult> SearchAsync(ExportProductSearchCriteria criteria)
        {
            var result = new ProductSearchResult();

            var resultByCategories = await _productSearchService.SearchProductsAsync(new ProductSearchCriteria()
            {
                ProductTypes = new[] { _physicalProductType },
                CatalogId = criteria.CatalogId,
                CategoryIds = criteria.CategoryIds,
                SearchInChildren = true,
                SearchInVariations = true,
                Take = criteria.Take,
                Skip = criteria.Skip,
            });

            result.Results = resultByCategories.Results;
            result.TotalCount = resultByCategories.TotalCount;

            var totalCount = resultByCategories.TotalCount;
            var skip = Math.Min(totalCount, criteria.Skip);
            var take = Math.Min(criteria.Take, Math.Max(0, totalCount - criteria.Skip));

            var resultByItems = await _productSearchService.SearchProductsAsync(new ProductSearchCriteria()
            {
                ProductTypes = new[] { _physicalProductType },
                CatalogId = criteria.CatalogId,
                ObjectIds = criteria.CategoryIds,
                SearchInChildren = true,
                SearchInVariations = true,
                Take = take,
                Skip = skip,
            });

            result.Results.AddRange(resultByItems.Results);
            result.TotalCount += resultByItems.TotalCount;

            return result;
        }
    }
}
