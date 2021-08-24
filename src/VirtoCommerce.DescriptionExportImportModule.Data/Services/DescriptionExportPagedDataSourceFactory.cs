using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class DescriptionExportPagedDataSourceFactory : IDescriptionExportPagedDataSourceFactory
    {
        private readonly IProductDescriptionSearchService _productDescriptionSearchService;

        public DescriptionExportPagedDataSourceFactory(IProductDescriptionSearchService productDescriptionSearchService)
        {
            _productDescriptionSearchService = productDescriptionSearchService;
        }

        public IDescriptionExportPagedDataSource Create(int pageSize, ExportDataRequest request)
        {
            return new DescriptionExportPagedDataSource(_productDescriptionSearchService, pageSize, request);
        }
    }
}
