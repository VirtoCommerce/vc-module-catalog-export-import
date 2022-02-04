using System.Globalization;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core.Services;

namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public sealed class ImportConfigurationFactory: IImportConfigurationFactory
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
