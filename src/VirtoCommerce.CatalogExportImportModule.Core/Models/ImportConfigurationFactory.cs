using System.Globalization;
using CsvHelper.Configuration;

namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public sealed class ImportConfigurationFactory
    {
        public CsvConfiguration Create()
        {
            var result = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                ReadingExceptionOccurred = args => false,
                BadDataFound = null,
                MissingFieldFound = null,
            };

            return result;
        }
    }
}
