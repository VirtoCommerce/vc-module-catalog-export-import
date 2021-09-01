using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class ExportPagedDataSourceFactory : IExportPagedDataSourceFactory
    {
        private readonly IProductEditorialReviewSearchService _productEditorialReviewSearchService;
        private readonly IItemService _itemService;
        private readonly IListEntrySearchService _listEntrySearchService;

        public ExportPagedDataSourceFactory(
            IProductEditorialReviewSearchService productEditorialReviewSearchService,
            IItemService itemService,
            IListEntrySearchService listEntrySearchService
            )
        {
            _productEditorialReviewSearchService = productEditorialReviewSearchService;
            _itemService = itemService;
            _listEntrySearchService = listEntrySearchService;
        }

        public IExportPagedDataSource Create(int pageSize, ExportDataRequest request)
        {
            return new ExportPagedDataSource(_productEditorialReviewSearchService, _itemService, _listEntrySearchService, pageSize, request);
        }
    }
}
