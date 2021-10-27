using System.Collections.Generic;

namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public sealed class ImportErrorsContext
    {
        public IList<ImportError> Errors { get; set; } = new List<ImportError>();
    }
}
