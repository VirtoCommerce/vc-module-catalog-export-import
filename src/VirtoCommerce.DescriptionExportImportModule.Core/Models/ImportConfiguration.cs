using System.Globalization;
using CsvHelper.Configuration;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Models
{
    public class ImportConfigurationFactory
    {
        public virtual CsvConfiguration Create()
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
