using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Core.Models
{
    public class ProductEditorialReviewSearchCriteria : SearchCriteriaBase
    {
        public string CatalogId { get; set; }
        public string CategoryId { get; set; }
        public string[] CategoryIds { get; set; }
        public string[] ItemIds { get; set; }
        public bool DeepSearch { get; set; }
    }
}
