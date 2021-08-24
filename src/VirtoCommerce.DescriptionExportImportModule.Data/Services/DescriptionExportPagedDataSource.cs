using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
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

        public EditorialReview[] Items { get; private set; }

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

        public Task<bool> FetchAsync()
        {
            return Task.FromResult(false);
        }
    }
}
