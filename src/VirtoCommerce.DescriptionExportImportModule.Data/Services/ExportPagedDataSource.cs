using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class ExportPagedDataSource : IExportPagedDataSource

    {
        private readonly IProductEditorialReviewSearchService _productEditorialReviewSearchService;
        private readonly IItemService _itemService;
        private readonly ExportDataRequest _exportRequest;

        public int CurrentPageNumber { get; private set; }

        public int PageSize { get; }

        public IExportable[] Items { get; private set; }

        public ExportPagedDataSource(
            IProductEditorialReviewSearchService productEditorialReviewSearchService,
            IItemService itemService,
            int pageSize,
            ExportDataRequest exportRequest
            )
        {
            _productEditorialReviewSearchService = productEditorialReviewSearchService;
            _itemService = itemService;

            _exportRequest = exportRequest;
            PageSize = pageSize;
        }

        public async Task<int> GetTotalCountAsync()
        {
            var searchCriteria = _exportRequest.ToSearchCriteria();
            searchCriteria.Take = 0;

            var searchResult = await _productEditorialReviewSearchService.SearchEditorialReviewsAsync(searchCriteria);

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

            var searchResult = await _productEditorialReviewSearchService.SearchEditorialReviewsAsync(searchCriteria);

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
                        DescriptionContent = x.Content,
                        Language = x.LanguageCode,
                        Type = x.ReviewType,
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
