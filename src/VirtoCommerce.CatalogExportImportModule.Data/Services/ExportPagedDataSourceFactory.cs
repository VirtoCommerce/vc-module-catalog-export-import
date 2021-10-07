using System;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogModule.Core.Services;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class ExportPagedDataSourceFactory : IExportPagedDataSourceFactory
    {
        private readonly IProductEditorialReviewSearchService _productEditorialReviewSearchService;
        private readonly IItemService _itemService;
        private readonly IExportProductSearchService _productSearchService;

        public ExportPagedDataSourceFactory(
            IProductEditorialReviewSearchService productEditorialReviewSearchService,
            IExportProductSearchService productSearchService,
            IItemService itemService
            )
        {
            _productEditorialReviewSearchService = productEditorialReviewSearchService;
            _itemService = itemService;
            _productSearchService = productSearchService;
        }

        public IExportPagedDataSource Create(int pageSize, ExportDataRequest request)
        {
            switch (request.DataType)
            {
                case ModuleConstants.DataTypes.EditorialReview:
                    return new EditorialReviewExportPagedDataSource(_productEditorialReviewSearchService, _itemService, pageSize, request);
                case ModuleConstants.DataTypes.PhysicalProduct:
                    return new ProductExportPagedDataSource(_productSearchService, pageSize, request);
                default:
                    throw new ArgumentException(nameof(request.DataType));
            }
        }
    }
}
