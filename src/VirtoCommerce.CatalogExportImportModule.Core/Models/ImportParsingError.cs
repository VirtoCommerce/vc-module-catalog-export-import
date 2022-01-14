using System.Collections.Generic;

namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public sealed class ImportParsingError: ImportError
    {
        public string ErrorCode { get; set; }

        public IEnumerable<string> Parameters { get; set; }
    }
}
