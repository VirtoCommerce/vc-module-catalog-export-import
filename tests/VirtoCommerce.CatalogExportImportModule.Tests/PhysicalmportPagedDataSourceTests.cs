using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CustomerExportImportModule.Tests;
using Xunit;

namespace VirtoCommerce.CatalogExportImportModule.Tests
{
    [Trait("Category", "CI")]
    public class PhysicalmportPagedDataSourceTests
    {
        private const string CsvFileName = "file.csv";
        private const string CsvHeader = "Product Name;Product Id;Product SKU;Product Outer Id;Category Id;Category Outer Id;Product Type;Priority;Gtin;Can be purchased;Visible;Track Inventory;Package Type;Max Quantity;Min Quantity;Manufacturer Part Number;Measure Unit;Weight Unit;Weight;Height;Length;Width;Tax Type;Shipping Type;Vendor;First Listed;Listing Expires On";
        private static readonly string[] CsvRecords =
        {
            "Samsung Galaxy Note 4 SM-N910C 32GB;;SAGN4N910CBK;GALAXYNOTE4;c6ae8a94-a2ec-4b49-9bda-4761de14225a;SMARTPHONES;Physical;1;;TRUE;TRUE;TRUE;;0;1;;;;;;;;;;;8/13/2015 13:50;",
            "Microsoft Lumia 640 XL RM-1065 8GB Dual SIM;1486f5a1a25f48a999189c081792a379;MIL640X4GLWH;;c6ae8a94-a2ec-4b49-9bda-4761de14225a;;Physical;0;;TRUE;FALSE;TRUE;;0;1;;;;;;;;;;;8/13/2015 14:43;",
            "iPhuck 10;;ZA-3478;PH0;;SMARTPHONES;Physical;0;;FALSE;TRUE;TRUE;;1;1;;;;;;;;;;;8/14/2075 13:50;"
        };

        [Theory]
        [MemberData(nameof(GetCsvWithAndWithoutHeader))]
        public void GetTotalCount_Calculate_AndReturnTotalCount(string[] records, string header)
        {
            // Arrange
            var csv = TestHelper.GetCsv(records, header);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
            using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create<CsvPhysicalProduct>(CsvFileName, 10);

            // Act
            var totalCount = customerImportPagedDataSource.GetTotalCount();

            // Assert
            Assert.Equal(3, totalCount);
        }

        [Fact]
        public void GetTotalCount_CacheTotalCount_AndReturnSameValue()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
            using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create<CsvPhysicalProduct>(CsvFileName, 10);

            // Act
            customerImportPagedDataSource.GetTotalCount();
            var totalCount = customerImportPagedDataSource.GetTotalCount();

            // Assert
            Assert.Equal(3, totalCount);
        }

        public static IEnumerable<object[]> GetCsvWithAndWithoutHeader()
        {
            yield return new object[] { CsvRecords, CsvHeader };
            yield return new object[] { CsvRecords, null };
        }

        [Fact]
        public async Task FetchAsync_WithMissedHeader_ThrowsException()
        {
            static async Task FetchAsync()
            {
                // Arrange
                var csv = TestHelper.GetCsv(CsvRecords);
                var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
                var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
                using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create<CsvPhysicalProduct>(CsvFileName, 10);

                // Act
                await customerImportPagedDataSource.FetchAsync();
            }

            // Assert
            await Assert.ThrowsAsync<HeaderValidationException>(FetchAsync);
        }

        [Fact]
        public async Task FetchAsync_WithSpecifiedPageSize_WillReturnSpecifiedNumberOfItems()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
            using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create<CsvPhysicalProduct>(CsvFileName, 1);

            // Act
            await customerImportPagedDataSource.FetchAsync();
            await customerImportPagedDataSource.FetchAsync();

            // Assert
            Assert.Equal("1486f5a1a25f48a999189c081792a379", customerImportPagedDataSource.Items.Single().Record.ProductId);
        }

        [Fact]
        public async Task FetchAsync_AfterGetTotalCount_WillStartReadingFromTheSamePosition()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
            using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create<CsvPhysicalProduct>(CsvFileName, 1);

            // Act
            await customerImportPagedDataSource.FetchAsync();
            customerImportPagedDataSource.GetTotalCount();
            await customerImportPagedDataSource.FetchAsync();

            // Assert
            Assert.Equal("1486f5a1a25f48a999189c081792a379", customerImportPagedDataSource.Items.Single().Record.ProductId);
        }

        [Fact]
        public async Task FetchAsync_BeforeEndOfCsvFile_WillReturnTrue()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
            using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create<CsvPhysicalProduct>(CsvFileName, 1);

            // Act
            var result = await customerImportPagedDataSource.FetchAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task FetchAsync_AfterEndOfCsvFile_WillReturnFalse()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
            using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create<CsvPhysicalProduct>(CsvFileName, 10);

            // Act
            await customerImportPagedDataSource.FetchAsync();
            var result = await customerImportPagedDataSource.FetchAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task FetchAsync_AfterEndOfCsvFile_WillFetchNoItems()
        {
            // Arrange
            var csv = TestHelper.GetCsv(CsvRecords, CsvHeader);
            var blobStorageProvider = TestHelper.GetBlobStorageProvider(csv);
            var customerImportPagedDataSourceFactory = TestHelper.GetCustomerImportPagedDataSourceFactory(blobStorageProvider);
            using var customerImportPagedDataSource = customerImportPagedDataSourceFactory.Create<CsvPhysicalProduct>(CsvFileName, 10);

            // Act
            await customerImportPagedDataSource.FetchAsync();
            await customerImportPagedDataSource.FetchAsync();

            // Assert
            Assert.Empty(customerImportPagedDataSource.Items);
        }
    }
}
