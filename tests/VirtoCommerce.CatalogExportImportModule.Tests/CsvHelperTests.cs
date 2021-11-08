using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Data.Helpers;
using Xunit;

namespace VirtoCommerce.CatalogExportImportModule.Tests
{
    public class CsvHelperTests
    {
        [Fact]
        public async Task CsvReader_ReadAsync_DoubleBadData_ErrorsFound()
        {
            // Arrange
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
            var records = new[] { "Test name;test SKU;;", "Test name 2;test SKU 2;;", "Test name 3;test SKU 3;\"Physical;" };
            var csv = TestHelper.GetCsv(records, header);
            var textReader = new StreamReader(TestHelper.GetStream(csv), leaveOpen: true);

            var csvReader = new CsvReader(textReader, csvConfiguration);

            // Act
            await csvReader.ReadAsync();
            csvReader.ReadHeader();

            while (await csvReader.ReadAsync())
            {
                csvReader.GetRecord<CsvPhysicalProduct>();

            }

            // Assert
            Assert.Equal(2, errorCount);
        }

        [Fact]
        public async Task CsvReader_ReadAsync_EnsureBadDataFoundWasCalledOnceCase()
        {
            // Arrange
            var errorCount = 0;
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ReadingExceptionOccurred = args => false,
                BadDataFound = args =>
                {
                    // Add error to error report
                    throw new BadDataException(args.Context, "Exception to prevent double BadDataFount call");
                },
                Delimiter = ";",
            };
            var header = "Product Name;Product SKU;Product Type;";
            var records = new[] { "Test name;\"test SKU\";;", "Test name 2;\"test SKU 2;;", "Test name 3;test SKU 3;;", "Test name 4;\"test SKU 4;;", };
            var csv = TestHelper.GetCsv(records, header);
            var textReader = new StreamReader(TestHelper.GetStream(csv), leaveOpen: true);

            // Act
            var csvReader = new CsvReader(textReader, csvConfiguration);
            var errorString = string.Empty;
            try
            {
                while (await csvReader.ReadAsync())
                {
                    csvReader.GetRecord<CsvPhysicalProduct>();
                }
            }
            catch (BadDataException e)
            {
                errorString = e.Context.Parser.RawRecord;
                ++errorCount;
            }

            // Assert
            Assert.Equal(1, errorCount);
            Assert.Equal($"Test name 2;\"test SKU 2;;{Environment.NewLine}Test name 3;test SKU 3;;{Environment.NewLine}Test name 4;\"test SKU 4;;", errorString.TrimEnd());
        }

        [Fact]
        public void CsvWriter_WriteRecordsWithProps_PropsReaded()
        {
            // Arrange
            var stream = new MemoryStream();

            using var streamWriter = new StreamWriter(stream)
            {
                AutoFlush = true
            };

            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

            csvWriter.Context.RegisterClassMap(new TestExportClassMap(new[] { "Property 1", "Property 2" }));

            var data = new TestExportClass
            {
                Id = "Test id 1",
                Properties = new List<TestPropertyExportClass>
                {
                    new TestPropertyExportClass
                    {
                        Name = "Property 1",
                        Value = "Property value 1",
                    },
                    new TestPropertyExportClass
                    {
                        Name = "Property 2",
                        Value = "Property value 2",
                    }
                }
            };

            // Act
            csvWriter.WriteRecords(new[] { data });
            stream.Position = 0;

            using var streamReader = new StreamReader(stream);

            var result = streamReader.ReadToEnd();

            // Assert
            Assert.Equal($"Id,Property 1,Property 2{csvWriter.Configuration.NewLine}Test id 1,Property value 1,Property value 2", result.TrimEnd());
        }

        [Theory]
        [InlineData("", 0)]
        [InlineData("55.4561, 25.5454", 1)]
        [InlineData("55.4561, 25.5454, 55.4561, 25.5454", 2)]
        [InlineData("55.4561,25.5454,55.4561,25.5454", 2)]
        public void CsvHelper_SplitGeoPointMultivalueString_Success(string valuesString, int expectedCount)
        {
            // Arrange

            // Act
            var result = CsvImportHelper.SplitGeoPointMultivalueString(valuesString);

            var resultCount = result.Count();

            // Assert
            Assert.Equal(expectedCount, resultCount);
        }

        public class TestExportClass
        {
            public string Id { get; set; }

            public ICollection<TestPropertyExportClass> Properties { get; set; }
        }

        public class TestPropertyExportClass
        {
            public string Value { get; set; }
            public string Name { get; set; }
        }

        public class TestExportClassMap : ClassMap<TestExportClass>
        {
            public TestExportClassMap(string[] additionalColumns)
            {
                AutoMap(CultureInfo.InvariantCulture);

                var propertiesInfo = ClassType.GetProperty(nameof(TestExportClass.Properties));
                var currentColumnIndex = MemberMaps.Count;

                foreach (var additionalColumn in additionalColumns)
                {
                    var memberMap = MemberMap.CreateGeneric(ClassType, propertiesInfo);
                    memberMap.Name(additionalColumn);
                    memberMap.Data.IsOptional = true;
                    memberMap.Data.Index = currentColumnIndex++;

                    Func<ConvertToStringArgs<TestExportClass>, string> func = xs =>
                    {
                        return xs.Value.Properties.First(x => x.Name == additionalColumn).Value;
                    };

                    memberMap.Data.WritingConvertExpression =
                        (Expression<Func<ConvertToStringArgs<TestExportClass>, string>>)(ex => func(ex));

                    MemberMaps.Add(memberMap);
                }
            }
        }
    }
}
