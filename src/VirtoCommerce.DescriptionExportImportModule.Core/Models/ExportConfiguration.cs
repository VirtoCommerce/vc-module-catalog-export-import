using System.Globalization;
using CsvHelper.Configuration;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Models
{
    public class ExportConfiguration : Configuration
    {
        public ExportConfiguration()
            : base(CultureInfo.InvariantCulture)
        {
        }

        public override string Delimiter { get; set; } = ";";
    }
}
