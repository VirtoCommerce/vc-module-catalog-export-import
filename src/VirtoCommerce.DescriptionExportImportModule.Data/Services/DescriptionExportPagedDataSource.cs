using System;
using System.Threading.Tasks;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class DescriptionExportPagedDataSource : IDescriptionExportPagedDataSource

    {
        private readonly IProductDescriptionSearchService _productDescriptionSearchService;
        private readonly ExportDataRequest _exportRequest;

        public int CurrentPageNumber { get; private set; }

        public int PageSize { get; }

        public IExportable[] Items { get; private set; }

        public DescriptionExportPagedDataSource(IProductDescriptionSearchService productDescriptionSearchService, int pageSize, ExportDataRequest exportRequest)
        {
            _productDescriptionSearchService = productDescriptionSearchService;
            _exportRequest = exportRequest;
            PageSize = pageSize;
        }

        public async Task<int> GetTotalCountAsync()
        {
            var searchCriteria = _exportRequest.ToSearchCriteria();
            searchCriteria.Take = 0;

            var searchResult = await _productDescriptionSearchService.SearchProductDescriptionsAsync(searchCriteria);

            return searchResult.TotalCount;
        }

        public async Task<bool> FetchAsync()
        {
            if (CurrentPageNumber * PageSize >= await GetTotalCountAsync())
            {
                Items = Array.Empty<IExportable>();
                return false;
            }

            var searchCriteria = _exportRequest.ToSearchCriteria();
            searchCriteria.Skip = CurrentPageNumber * PageSize;
            searchCriteria.Take = PageSize;

            var searchResult = await _productDescriptionSearchService.SearchProductDescriptionsAsync(searchCriteria);

            CurrentPageNumber++;
            return false;
        }
    }
}
