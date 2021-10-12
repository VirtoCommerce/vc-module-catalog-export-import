using VirtoCommerce.CatalogExportImportModule.Core;

namespace VirtoCommerce.CatalogExportImportModule.Data.Helpers
{
    public static class DataTypeExtensions
    {
        public static string ToPushNotificationTitle(this string dataType)
        {
            return ModuleConstants.PushNotificationsTitles.Export[dataType];
        }

        public static string ToExportFileNamePrefix(this string dataType)
        {
            return ModuleConstants.ExportFileNamePrefixes[dataType];
        }
    }
}
