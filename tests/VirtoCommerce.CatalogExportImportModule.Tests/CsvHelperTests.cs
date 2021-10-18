using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using Xunit;

namespace VirtoCommerce.CatalogExportImportModule.Tests
{
    public class CsvHelperTests
    {

        [Fact]
        public async Task TestDoubleBadDataFoundCase()
        {
            var errorCount = 0;
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ReadingExceptionOccurred = args => false,
                BadDataFound = args =>
                {
                    ++errorCount;
                },
                Delimiter = ";",
            };
            var header = "Product Name;Product SKU;Product Type;";
            var records = new[] { "Test name;test SKU;;", "Test name 2;test SKU 2;;", "Test name 3;\"test SKU 3;;" };
            var csv = TestHelper.GetCsv(records, header);
            var textReader = new StreamReader(TestHelper.GetStream(csv), leaveOpen: true);

            var csvReader = new CsvReader(textReader, csvConfiguration);

            await csvReader.ReadAsync();
            csvReader.ReadHeader();

            while (await csvReader.ReadAsync())
            {
                csvReader.GetRecord<CsvPhysicalProduct>();

            }

            Assert.Equal(2, errorCount);
        }

        [Fact]
        public async Task EnsureBadDataFoundWasCalledOnceCase()
        {
            var isRecordBad = false;
            var errorCount = 0;
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ReadingExceptionOccurred = args => false,
                BadDataFound = args =>
                {
                    ++errorCount;
                    isRecordBad = true;
                },
                Delimiter = ";",
            };
            var header = "Product Name;Product SKU;Product Type;";
            var records = new[] { "Test name;test SKU;;", "Test name 2;test SKU 2;;", "Test name 3;\"test SKU 3;;" };
            var csv = TestHelper.GetCsv(records, header);
            var textReader = new StreamReader(TestHelper.GetStream(csv), leaveOpen: true);

            var csvReader = new CsvReader(textReader, csvConfiguration);

            while (await csvReader.ReadAsync())
            {
                if (!isRecordBad)
                {
                    csvReader.GetRecord<CsvPhysicalProduct>();
                }

                isRecordBad = false;
            }

            Assert.Equal(1, errorCount);
        }
    }
}
