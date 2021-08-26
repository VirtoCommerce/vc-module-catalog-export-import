using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.DescriptionExportImportModule.Core
{
    public static class ModuleConstants
    {
        public const int KByte = 1024;

        public const int MByte = 1024 * KByte;

        public static class ValidationErrors
        {
            public const string DuplicateError = "Duplicate";

            public const string FileNotExisted = "file-not-existed";

            public const string NoData = "no-data";

            public const string ExceedingFileMaxSize = "exceeding-file-max-size";

            public const string WrongDelimiter = "wrong-delimiter";

            public const string ExceedingLineLimits = "exceeding-line-limits";

            public const string MissingRequiredColumns = "missing-required-columns";

            public const string MissingRequiredValues = "missing-required-values";

            public const string ExceedingMaxLength = "exceeding-max-length";

            public const string ArrayValuesExceedingMaxLength = "array-values-exceeding-max-length";

            public const string InvalidValue = "invalid-value";

            public const string NotUniqueValue = "not-unique-value";
        }

        public static class Security
        {
            public static class Permissions
            {
                public const string ImportAccess = "product:description:import";

                public static string[] AllPermissions { get; } = { ImportAccess };
            }
        }

        public static class Settings
        {
            public const int PageSize = 50;

            public static class General
            {
                public static SettingDescriptor ImportLimitOfLines { get; } = new SettingDescriptor
                {
                    Name = "DescriptionExportImport.Import.LimitOfLines",
                    GroupName = "DescriptionExportImport|Import",
                    ValueType = SettingValueType.PositiveInteger,
                    IsHidden = true,
                    DefaultValue = 10000
                };

                public static SettingDescriptor ImportFileMaxSize { get; } = new SettingDescriptor
                {
                    Name = "DescriptionExportImport.Import.FileMaxSize",
                    GroupName = "DescriptionExportImport|Import",
                    ValueType = SettingValueType.PositiveInteger,
                    IsHidden = true,
                    DefaultValue = 1 // MB
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        return new List<SettingDescriptor>
                        {
                            ImportLimitOfLines,
                            ImportFileMaxSize
                        };
                    }
                }
            }
        }
    }
}
