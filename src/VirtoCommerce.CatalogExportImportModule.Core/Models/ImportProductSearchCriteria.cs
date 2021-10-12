using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public class ImportProductSearchCriteria : SearchCriteriaBase
    {
        public string[] OuterIds { get; set; }
    }
}
