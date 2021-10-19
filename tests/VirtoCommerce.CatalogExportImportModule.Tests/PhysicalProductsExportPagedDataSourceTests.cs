using System;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Moq;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogExportImportModule.Data.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using Xunit;

namespace VirtoCommerce.CatalogExportImportModule.Tests
{
    public class PhysicalProductsExportPagedDataSourceTests
    {
        public PhysicalProductsExportPagedDataSourceTests()
        {
        }

        [Fact]
        public async Task GetTotalCount_Calculate_AndReturnTotalCount()
        {
            // Arrange
            var products = new Faker<CatalogProduct>().Generate(2).ToArray();
            var dataSourceFactory = GetExportPagedDataSourceFactory(products);

            var request = new ExportDataRequest() { DataType = ModuleConstants.DataTypes.PhysicalProduct };

            var dataSource = dataSourceFactory.Create(10, request);

            // Act
            var totalCount = await dataSource.GetTotalCountAsync();

            // Assert
            Assert.Equal(2, totalCount);
        }

        [Fact]
        public async Task FetchAsync_MultipleTimes_WillUpdateCurrentPageNumber()
        {
            // Arrange
            var productsCount = 25;
            var products = new Faker<CatalogProduct>().Generate(productsCount).ToArray();
            var factory = GetExportPagedDataSourceFactory(products);
            var request = new ExportDataRequest() { DataType = ModuleConstants.DataTypes.PhysicalProduct };
            var dataSource = factory.Create(10, request);

            // Act
            await dataSource.FetchAsync();
            await dataSource.FetchAsync();

            // Assert
            Assert.Equal(2, dataSource.CurrentPageNumber);
        }

        [Fact]
        public async Task FetchAsync_WithSpecifiedPageSize_ReturnsOnlyRequestedNumberOfItems()
        {
            // Arrange
            var productsCount = 15;
            var products = new Faker<CatalogProduct>().Generate(productsCount).ToArray();
            var factory = GetExportPagedDataSourceFactory(products);
            var request = new ExportDataRequest() { DataType = ModuleConstants.DataTypes.PhysicalProduct };
            var dataSource = factory.Create(10, request);

            // Act
            await dataSource.FetchAsync();

            // Assert
            Assert.Equal(10, dataSource.Items.Length);

        }

        [Fact]
        public async Task FetchAsync_BeforeEndOfTheSource_WillReturnTrue()
        {
            // Arrange
            var productsCount = 5;
            var products = new Faker<CatalogProduct>().Generate(productsCount).ToArray();
            var factory = GetExportPagedDataSourceFactory(products);
            var request = new ExportDataRequest() { DataType = ModuleConstants.DataTypes.PhysicalProduct };
            var dataSource = factory.Create(10, request);

            // Act
            var result = await dataSource.FetchAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task FetchAsync_BeforeEndOfTheSource_WillReturnFalse()
        {
            // Arrange
            var productsCount = 15;
            var products = new Faker<CatalogProduct>().Generate(productsCount).ToArray();
            var factory = GetExportPagedDataSourceFactory(products);
            var request = new ExportDataRequest() { DataType = ModuleConstants.DataTypes.PhysicalProduct };
            var dataSource = factory.Create(10, request);

            // Act
            var result = await dataSource.FetchAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task FetchAsync_AfterEndOfTheSource_WillFetchNoItems()
        {
            // Arrange
            var products = Array.Empty<CatalogProduct>();
            var factory = GetExportPagedDataSourceFactory(products);
            var request = new ExportDataRequest() { DataType = ModuleConstants.DataTypes.PhysicalProduct };
            var dataSource = factory.Create(10, request);


            // Act
            await dataSource.FetchAsync();
            await dataSource.FetchAsync();
            await dataSource.FetchAsync();

            // Assert
            Assert.Empty(dataSource.Items);
        }


        //private methods

        private static IExportPagedDataSourceFactory GetExportPagedDataSourceFactory(CatalogProduct[] products)
        {
            var productSearchService = GetExportProductSearchService(products);

            IExportPagedDataSource DataSourceCreatorFunc(ExportDataRequest request, int pageSize) => new ProductExportPagedDataSource(productSearchService, pageSize, request);

            return new ExportPagedDataSourceFactory(new[]
            {
                (Func<ExportDataRequest, int, IExportPagedDataSource>) DataSourceCreatorFunc
            });
        }


        private static IExportProductSearchService GetExportProductSearchService(CatalogProduct[] products)
        {
            var mock = new Mock<IExportProductSearchService>();
            mock.Setup(service => service.SearchAsync(It.IsAny<ExportProductSearchCriteria>()))
                .ReturnsAsync((ExportProductSearchCriteria criteria) => new ProductSearchResult
                {
                    Results = products.Skip(criteria.Skip).Take(criteria.Take).ToArray(),
                    TotalCount = products.Length
                });
            return mock.Object;
        }
    }
}
