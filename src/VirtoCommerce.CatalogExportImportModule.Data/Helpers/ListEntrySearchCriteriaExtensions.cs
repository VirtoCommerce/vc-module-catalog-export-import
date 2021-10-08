using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Data.Helpers
{
    public static class ExportDataRequestExtensions
    {
        public static ExportProductSearchCriteria ToExportProductSearchCriteria(this ExportDataRequest dataRequest)
        {
            return new ExportProductSearchCriteria()
            {
                CatalogId = dataRequest.CatalogId,
                CategoryIds = dataRequest.CategoryIds,
                ItemIds = dataRequest.ItemIds,
            };
        }
    }
}
