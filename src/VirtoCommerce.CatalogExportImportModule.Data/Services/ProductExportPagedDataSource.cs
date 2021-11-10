using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogExportImportModule.Data.Helpers;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class ProductExportPagedDataSource : IExportPagedDataSource
    {
        private readonly IExportProductSearchService _productSearchService;
        private readonly ExportDataRequest _exportRequest;
        private readonly IItemService _itemService;

        public int CurrentPageNumber { get; private set; }
        public int PageSize { get; }

        public string DataType => ModuleConstants.DataTypes.PhysicalProduct;

        public IExportable[] Items { get; private set; }

        public ProductExportPagedDataSource(IExportProductSearchService productSearchService, int pageSize, ExportDataRequest request, IItemService itemService)
        {
            _productSearchService = productSearchService;
            PageSize = pageSize;
            _exportRequest = request;
            _itemService = itemService;
        }

        public async Task<int> GetTotalCountAsync()
        {
            var searchCriteria = _exportRequest.ToExportProductSearchCriteria();
            searchCriteria.Take = 0;
            searchCriteria.SearchInVariations = true;

            var searchResult = await _productSearchService.SearchAsync(searchCriteria);

            return searchResult.TotalCount;
        }

        public async Task<bool> FetchAsync()
        {
            if (CurrentPageNumber * PageSize >= await GetTotalCountAsync())
            {
                Items = Array.Empty<IExportable>();
                return false;
            }

            var searchCriteria = _exportRequest.ToExportProductSearchCriteria();

            searchCriteria.Skip = CurrentPageNumber * PageSize;
            searchCriteria.Take = PageSize;
            searchCriteria.SearchInVariations = true;

            var searchResult = await _productSearchService.SearchAsync(searchCriteria);

            var mainProducts = await _itemService.GetByIdsAsync(searchResult.Results.Where(x => !string.IsNullOrEmpty(x.MainProductId)).Select(x => x.MainProductId).ToArray(), ItemResponseGroup.ItemInfo.ToString());

            Items = searchResult.Results.Select(x =>
            {
                var result = new CsvPhysicalProduct().FromModel(x);
                result.MainProductOuterId = mainProducts.FirstOrDefault(mp => mp.Id.EqualsInvariant(x.MainProductId))?.OuterId;

                return result;
            }).ToArray<IExportable>();

            CurrentPageNumber++;

            return true;
        }
    }
}
