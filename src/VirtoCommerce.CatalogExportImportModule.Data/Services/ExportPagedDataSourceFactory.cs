using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class ExportPagedDataSourceFactory : IExportPagedDataSourceFactory
    {
        private readonly IProductEditorialReviewSearchService _productEditorialReviewSearchService;
        private readonly IItemService _itemService;

        public ExportPagedDataSourceFactory(
            IProductEditorialReviewSearchService productEditorialReviewSearchService,
            IItemService itemService
            )
        {
            _productEditorialReviewSearchService = productEditorialReviewSearchService;
            _itemService = itemService;
        }

        public IExportPagedDataSource Create(int pageSize, ExportDataRequest request)
        {
            return new ExportPagedDataSource(_productEditorialReviewSearchService, _itemService, pageSize, request);
        }
    }
}