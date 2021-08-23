using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.DescriptionExportImportModule.Core
{
    public static class ModuleConstants
    {
        public static class Security
        {
            public static class Permissions
            {
                public const string Access = "virtoCommerceDescriptionExportImport:access";
                public const string Create = "virtoCommerceDescriptionExportImport:create";
                public const string Read = "virtoCommerceDescriptionExportImport:read";
                public const string Update = "virtoCommerceDescriptionExportImport:update";
                public const string Delete = "virtoCommerceDescriptionExportImport:delete";

                public static string[] AllPermissions { get; } = { Read, Create, Access, Update, Delete };
            }
        }

        public static class Settings
        {
            public static class General
            {
                public static SettingDescriptor VirtoCommerceDescriptionExportImportEnabled { get; } = new SettingDescriptor
                {
                    Name = "VirtoCommerceDescriptionExportImport.VirtoCommerceDescriptionExportImportEnabled",
                    GroupName = "VirtoCommerceDescriptionExportImport|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false
                };

                public static SettingDescriptor VirtoCommerceDescriptionExportImportPassword { get; } = new SettingDescriptor
                {
                    Name = "VirtoCommerceDescriptionExportImport.VirtoCommerceDescriptionExportImportPassword",
                    GroupName = "VirtoCommerceDescriptionExportImport|Advanced",
                    ValueType = SettingValueType.SecureString,
                    DefaultValue = "qwerty"
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        yield return VirtoCommerceDescriptionExportImportEnabled;
                        yield return VirtoCommerceDescriptionExportImportPassword;
                    }
                }
            }

            public static IEnumerable<SettingDescriptor> AllSettings
            {
                get
                {
                    return General.AllSettings;
                }
            }
        }
    }
}
