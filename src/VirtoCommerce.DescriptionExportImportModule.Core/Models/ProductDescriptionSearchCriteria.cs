using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Models
{
    public class ProductEditorialReviewSearchCriteria : SearchCriteriaBase
    {
        public string CatalogId { get; set; }
        public string CategoryId { get; set; }
    }
}
