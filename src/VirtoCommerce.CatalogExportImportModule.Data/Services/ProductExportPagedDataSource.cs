using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogExportImportModule.Data.Helpers;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class ProductExportPagedDataSource : IExportPagedDataSource
    {
        private readonly IExportProductSearchService _productSearchService;
        private readonly ExportDataRequest _exportRequest;

        public int CurrentPageNumber { get; private set; }
        public int PageSize { get; }

        public string DataType => ModuleConstants.DataTypes.PhysicalProduct;

        public IExportable[] Items { get; private set; }

        public ProductExportPagedDataSource(IExportProductSearchService productSearchService, int pageSize, ExportDataRequest request)
        {
            _productSearchService = productSearchService;
            PageSize = pageSize;
            _exportRequest = request;
        }

        public async Task<int> GetTotalCountAsync()
        {
            var searchCriteria = _exportRequest.ToExportProductSearchCriteria();
            searchCriteria.Take = 0;

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

            var searchResult = await _productSearchService.SearchAsync(searchCriteria);

            Items = searchResult.Results.Select(x => new CsvPhysicalProduct().FromModel(x)).ToArray<IExportable>();

            CurrentPageNumber++;

            return true;
        }
    }
}
