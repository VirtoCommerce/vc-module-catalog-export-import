using System.Globalization;
using CsvHelper.Configuration;

namespace VirtoCommerce.CatalogExportImportModule.Core.Models
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
