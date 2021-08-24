using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Models
{
    public class ProductDescriptionSearchCriteria : SearchCriteriaBase
    {
        public string CatalogId { get; set; }
        public string CategoryId { get; set; }
    }
}
