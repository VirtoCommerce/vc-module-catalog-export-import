using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class DescriptionExportPagedDataSourceFactory : IDescriptionExportPagedDataSourceFactory
    {
        private readonly IProductDescriptionSearchService _productDescriptionSearchService;
        private readonly IItemService _itemService;

        public DescriptionExportPagedDataSourceFactory(
            IProductDescriptionSearchService productDescriptionSearchService,
            IItemService itemService
            )
        {
            _productDescriptionSearchService = productDescriptionSearchService;
            _itemService = itemService;
        }

        public IDescriptionExportPagedDataSource Create(int pageSize, ExportDataRequest request)
        {
            return new DescriptionExportPagedDataSource(_productDescriptionSearchService, _itemService, pageSize, request);
        }
    }
}
