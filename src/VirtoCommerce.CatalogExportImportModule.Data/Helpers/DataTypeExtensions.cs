using VirtoCommerce.CatalogExportImportModule.Core;

namespace VirtoCommerce.CatalogExportImportModule.Data.Helpers
{
    public static class DataTypeExtensions
    {
        public static string ToImportPushNotificationTitle(this string dataType)
        {
            return ModuleConstants.PushNotificationsTitles.Import[dataType];
        }

        public static string ToExportPushNotificationTitle(this string dataType)
        {
            return ModuleConstants.PushNotificationsTitles.Export[dataType];
        }

        public static string ToExportFileNamePrefix(this string dataType)
        {
            return ModuleConstants.ExportFileNamePrefixes[dataType];
        }
    }
}
