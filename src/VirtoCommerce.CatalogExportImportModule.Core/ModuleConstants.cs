using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogExportImportModule.Core
{
    public static class ModuleConstants
    {
        public static class DataTypes
        {
            public const string EditorialReview = nameof(EditorialReview);

            public const string PhysicalProduct = "PhysicalProduct";
        }
        public static class ProductTypes
        {
            public const string Physical = "Physical";
        }

        public static class PushNotificationsTitles
        {
            public static readonly IReadOnlyDictionary<string, string> Export = new Dictionary<string, string>()
            {
                {DataTypes.EditorialReview, "Product descriptions export"},
                {DataTypes.PhysicalProduct, "Physical products export"},
            };
            public static readonly IReadOnlyDictionary<string, string> Import = new Dictionary<string, string>()
            {
                {DataTypes.EditorialReview, "Product descriptions import"},
                {DataTypes.PhysicalProduct, "Physical products import"},
            };
        }

        public static readonly IReadOnlyDictionary<string, string> ExportFileNamePrefixes = new Dictionary<string, string>()
        {
            {DataTypes.EditorialReview, "Descriptions"},
            {DataTypes.PhysicalProduct, "Physical_products"},
        };

        public const int ElasticMaxTake = 10000;

        public const int KByte = 1024;

        public const int MByte = 1024 * KByte;

        public static class ParsingErrorCodes
        {
            public const string NotEscapedQuote = nameof(NotEscapedQuote);
            public const string InvalidValue = nameof(InvalidValue);
            public const string MissedRequiredValue = nameof(MissedRequiredValue);
            public const string MissedValue = nameof(MissedValue);
        }

        public static string GetParsingErrorMessage(string errorCode, params object[] parameters)
        {
            switch (errorCode)
            {
                case ParsingErrorCodes.NotEscapedQuote:
                    return "This row has invalid data. The data after field with not escaped quote was lost.";
                case ParsingErrorCodes.InvalidValue:
                    return string.Format("This row has invalid value in the column '{0}'.", parameters.ToArray());
                case ParsingErrorCodes.MissedRequiredValue:
                    return parameters.Length > 1
                        ? string.Format("The required value in column {0} is missing.", parameters)
                        : $"The required values in columns {string.Join(", ", parameters)} are missing";
                case ParsingErrorCodes.MissedValue:
                    return $"This row has missed column{(parameters.Length > 1 ? "s" : string.Empty)}: {string.Join(", ", parameters)}";
                default:
                    return string.Empty;
            }
        }

        public static class ValidationContextData
        {
            public const string CatalogId = nameof(CatalogId);

            public const string ExistedCategories = nameof(ExistedCategories);

            public const string ExistedProducts = nameof(ExistedProducts);

            public const string Skus = nameof(Skus);

            public const string DuplicatedImportReview = nameof(DuplicatedImportReview);

            public const string AvailablePackageTypes = nameof(AvailablePackageTypes);

            public const string AvailableMeasureUnits = nameof(AvailableMeasureUnits);

            public const string AvailableWeightUnits = nameof(AvailableWeightUnits);

            public const string AvailableTaxTypes = nameof(AvailableTaxTypes);

            public const string AvailableLanguages = nameof(AvailableLanguages);

            public const string AvailableReviewTypes = nameof(AvailableReviewTypes);

            public const string ExistedReviews = nameof(ExistedReviews);

            public const string ExistingMainProducts = nameof(ExistingMainProducts);

            public const string ExistedProductsWithSameSku = nameof(ExistedProductsWithSameSku);
        }

        public static class ValidationErrorCodes
        {
            public const string DuplicateError = "duplicate";

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

            public const string ReviewExistsInSystem = "review-exists-in-system";

            public const string NotUniqueMultiValue = "not-unique-multi-value";

            public const string MainProductIsNotExists = "main-product-is-not-exists";

            public const string CycleSelfReference = "cycle-self-reference";

            public const string MainProductIsVariation = "main-product-is-variation";

            public const string ProductWithSameOuterIdExists = "product-with-same-outer-id-exists";

            public const string ProductDoesNotBelongToCatalog = "product-does-not-belong-to-catalog";

            public const string CategoryDoesNotExist = "category-does-not-exist";

            public const string CategoryDoesNotBelongToCatalog = "category-does-not-belong-to-catalog";

            public const string ProductWithSameSkuExists = "product-with-same-sku-exists";
        }

        public static readonly IReadOnlyDictionary<string, string> ValidationErrorMessages = new Dictionary<string, string>
        {
            { ValidationErrorCodes.MissingRequiredValues, "The required value in column '{0}' is missing." },
            { ValidationErrorCodes.ExceedingMaxLength, "Value in column '{0}' may have maximum {1} characters." },
            { ValidationErrorCodes.ArrayValuesExceedingMaxLength, "Every value in column '{0}' may have maximum {1} characters. The number of values is unlimited." },
            { ValidationErrorCodes.InvalidValue, "This row has invalid value in the column '{0}'." },
            { ValidationErrorCodes.NotUniqueValue, "Value in column '{0}' should be unique." },
            { ValidationErrorCodes.NotUniqueMultiValue, "Values in column '{0}' should be unique for the item." },
            { ValidationErrorCodes.MainProductIsNotExists, "The main product does not exist." },
            { ValidationErrorCodes.CycleSelfReference, "The main product id is the same as product. It means self cycle reference." },
            { ValidationErrorCodes.MainProductIsVariation, "The main product is variation. You should not import variations for variations." },
            { ValidationErrorCodes.ProductWithSameOuterIdExists, "Another product with the same Outer Id exists in the system." },
            { ValidationErrorCodes.ProductDoesNotBelongToCatalog, "The product does not belong to the catalog specified in the request." },
            { ValidationErrorCodes.CategoryDoesNotExist, "Such category does not exist in the system." },
            { ValidationErrorCodes.CategoryDoesNotBelongToCatalog, "The category does not belong to the catalog specified in the request." },
            { ValidationErrorCodes.ProductWithSameSkuExists, "Product with the same SKU and with different Id already exists in the current catalog." }
        };

        public static class Features
        {
            public const string CatalogExportImport = "CatalogExportImport";
        }

        public static class Security
        {
            public static class Permissions
            {
                public const string ImportAccess = "product:catalog:import";

                public const string ExportAccess = "product:catalog:export";

                public static string[] AllPermissions { get; } = { ImportAccess, ExportAccess };
            }
        }

        public static class Settings
        {
            public const int PageSize = 50;

            public static class General
            {
                public static SettingDescriptor ImportLimitOfLines { get; } = new SettingDescriptor
                {
                    Name = "CatalogExportImport.Import.LimitOfLines",
                    GroupName = "CatalogExportImport|Import",
                    ValueType = SettingValueType.PositiveInteger,
                    IsHidden = true,
                    DefaultValue = 10000
                };

                public static SettingDescriptor ImportFileMaxSize { get; } = new SettingDescriptor
                {
                    Name = "CatalogExportImport.Import.FileMaxSize",
                    GroupName = "CatalogExportImport|Import",
                    ValueType = SettingValueType.PositiveInteger,
                    IsHidden = true,
                    DefaultValue = 1 // MB
                };

                public static SettingDescriptor ExportLimitOfLines { get; } = new SettingDescriptor
                {
                    Name = "CatalogExportImport.Export.LimitOfLines",
                    GroupName = "CatalogExportImport|Export",
                    ValueType = SettingValueType.PositiveInteger,
                    IsHidden = true,
                    DefaultValue = 10000
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        return new List<SettingDescriptor>
                        {
                            ImportLimitOfLines,
                            ImportFileMaxSize,
                            ExportLimitOfLines
                        };
                    }
                }
            }
        }
    }
}
