using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogExportImportModule.Data.Helpers;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class EditorialReviewExportPagedDataSource : IExportPagedDataSource

    {
        private readonly IProductEditorialReviewSearchService _productEditorialReviewSearchService;
        private readonly IItemService _itemService;


        public int CurrentPageNumber { get; private set; }

        public int PageSize { get; set; }
        public ExportDataRequest Request { get; set; }

        public string DataType => ModuleConstants.DataTypes.EditorialReview;

        public IExportable[] Items { get; private set; }

        public EditorialReviewExportPagedDataSource(
            IProductEditorialReviewSearchService productEditorialReviewSearchService,
            IItemService itemService
            )
        {
            _productEditorialReviewSearchService = productEditorialReviewSearchService;
            _itemService = itemService;
        }

        public async Task<int> GetTotalCountAsync()
        {
            var searchCriteria = Request.ToExportProductSearchCriteria();
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

            var searchCriteria = Request.ToExportProductSearchCriteria();

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
                        DescriptionId = x.Id,
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
