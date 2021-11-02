using System;
using Moq;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogExportImportModule.Data.Services;
using VirtoCommerce.CatalogModule.Core.Services;
using Xunit;

namespace VirtoCommerce.CatalogExportImportModule.Tests
{
    public class ExportPagedDataSourceFactoryTests
    {
        [Theory]
        [InlineData(ModuleConstants.DataTypes.EditorialReview)]
        [InlineData(ModuleConstants.DataTypes.PhysicalProduct)]
        public void Create_RigthDataType_ReturnRigthDataSource(string dataType)
        {
            //average
            var factory = GetExportPagedDataSourceFactory();
            var request = new ExportDataRequest() { DataType = dataType };

            //action
            var dataSource = factory.Create(10, request);

            //assert
            Assert.Equal(dataType, dataSource.DataType);
        }

        [Fact]
        public void Create_WrongDataType_ThrowArgException()
        {
            //average
            var dataType = "invalid data type";
            var factory = GetExportPagedDataSourceFactory();
            var request = new ExportDataRequest() { DataType = dataType };

            //action + assert
            Assert.Throws<ArgumentException>(() => factory.Create(10, request));
        }


        private static IExportPagedDataSourceFactory GetExportPagedDataSourceFactory()
        {
            var productSearchService = new Mock<IExportProductSearchService>().Object;
            var reviewSearchService = new Mock<IProductEditorialReviewSearchService>().Object;
            var itemService = new Mock<IItemService>().Object;

            IExportPagedDataSource ProductDataSourceCreatorFunc(ExportDataRequest request, int pageSize) => new ProductExportPagedDataSource(productSearchService, pageSize, request, itemService);

            IExportPagedDataSource ReviewDataSourceCreatorFunc(ExportDataRequest request, int pageSize) => new EditorialReviewExportPagedDataSource(reviewSearchService, itemService, pageSize, request);

            return new ExportPagedDataSourceFactory((new Func<ExportDataRequest, int, IExportPagedDataSource>[]
            {
                ProductDataSourceCreatorFunc,
                ReviewDataSourceCreatorFunc
            }));
        }
    }
}
