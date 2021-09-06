using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Models
{
    public class ProductEditorialReviewSearchCriteria : SearchCriteriaBase
    {
        public string CatalogId { get; set; }
        public string[] CategoryIds { get; set; }
        public string[] ItemIds { get; set; }
    }
}
