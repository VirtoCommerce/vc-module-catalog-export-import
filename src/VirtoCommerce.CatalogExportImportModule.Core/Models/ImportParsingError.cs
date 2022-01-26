using System.Collections.Generic;
using System.Linq;

namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public sealed class ImportParsingError: ImportError
    {
        public string ErrorCode { get; set; }

        public IEnumerable<string> Parameters { get; set; } = Enumerable.Empty<string>();
    }
}
