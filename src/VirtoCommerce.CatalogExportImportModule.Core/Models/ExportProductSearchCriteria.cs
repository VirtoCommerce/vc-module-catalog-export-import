using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public sealed class ExportProductSearchCriteria : SearchCriteriaBase
    {
        public string CatalogId { get; set; }
        public string[] CategoryIds { get; set; }
        public string[] ItemIds { get; set; }
        public bool SearchInVariations { get; set; }
    }
}
