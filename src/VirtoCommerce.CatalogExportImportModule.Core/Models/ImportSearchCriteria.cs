using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public class ImportSearchCriteria : SearchCriteriaBase
    {
        public string[] OuterIds { get; set; }
    }
}
