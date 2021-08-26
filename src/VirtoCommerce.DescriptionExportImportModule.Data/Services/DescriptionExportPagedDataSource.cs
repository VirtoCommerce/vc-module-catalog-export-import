using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class DescriptionExportPagedDataSource : IDescriptionExportPagedDataSource

    {
        private readonly IProductDescriptionSearchService _productDescriptionSearchService;
        private readonly IItemService _itemService;
        private readonly ExportDataRequest _exportRequest;

        public int CurrentPageNumber { get; private set; }

        public int PageSize { get; }

        public IExportable[] Items { get; private set; }

        public DescriptionExportPagedDataSource(
            IProductDescriptionSearchService productDescriptionSearchService,
            IItemService itemService,
            int pageSize,
            ExportDataRequest exportRequest
            )
        {
            _productDescriptionSearchService = productDescriptionSearchService;
            _itemService = itemService;

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

            Items = searchResult
                .Results
                .OfType<ExtendedEditorialReview>()
                .Select(async x =>
                {
                    var product = await _itemService.GetByIdAsync(x.ItemId, ItemResponseGroup.ItemInfo.ToString());
                    var result = new CsvEditorialReview
                    {
                        ProductName = product.Name,
                        ProductSku = product.Code,
                        Id = x.Id,
                        Content = x.Content,
                        LanguageCode = x.LanguageCode,
                        ReviewType = x.ReviewType,
                    };

                    return result;
                })
                .Select(x => x.Result)
                .ToArray<IExportable>();

            CurrentPageNumber++;

            return true;
        }
    }
}
