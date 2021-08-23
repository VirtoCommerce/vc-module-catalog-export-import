using System;
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
                public static string[] AllPermissions { get; } = { };
            }
        }

        public static class Settings
        {
            public static class General
            {
                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        return Array.Empty<SettingDescriptor>();
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
