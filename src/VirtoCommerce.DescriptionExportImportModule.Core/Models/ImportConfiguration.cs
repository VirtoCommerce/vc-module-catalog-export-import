using System.Globalization;
using CsvHelper.Configuration;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Models
{
    public static class ImportConfiguration
    {
        public static CsvConfiguration GetCsvConfiguration()
        {
            var result = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ReadingExceptionOccurred = args => false,
                BadDataFound = args => { },
                Delimiter = ";",
            };

            return result;
        }
    }
}
