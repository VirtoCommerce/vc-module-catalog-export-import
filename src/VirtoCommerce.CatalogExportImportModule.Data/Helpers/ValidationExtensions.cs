using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogExportImportModule.Data.Helpers
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, TProperty> WithImportState<T, TRecord, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, ImportRecord<TRecord> importRecord)
        {
            return rule.WithState(_ => new ImportValidationState<TRecord> { InvalidRecord = importRecord });
        }

        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithImportState<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule)
        {
            return rule.WithState(importRecord => new ImportValidationState<T> { InvalidRecord = importRecord });
        }

        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithMissingRequiredValueCodeAndMessage<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule, string columnName)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrorCodes.MissingRequiredValues)
                .WithMessage(string.Format(ModuleConstants.ValidationErrorMessages[ModuleConstants.ValidationErrorCodes.MissingRequiredValues], columnName));
        }

        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithExceededMaxLengthCodeAndMessage<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule, string columnName, int maxLength)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrorCodes.ExceedingMaxLength)
                .WithMessage(string.Format(ModuleConstants.ValidationErrorMessages[ModuleConstants.ValidationErrorCodes.ExceedingMaxLength], columnName, maxLength));
        }

        public static IRuleBuilderOptions<PropertyValue, TProperty> WithExceededMaxLengthCodeAndMessage<TProperty>(this IRuleBuilderOptions<PropertyValue, TProperty> rule, int maxLength)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrorCodes.ExceedingMaxLength)
                .WithMessage(dynamicPropertyValue => string.Format(ModuleConstants.ValidationErrorMessages[ModuleConstants.ValidationErrorCodes.ExceedingMaxLength], dynamicPropertyValue.PropertyName, maxLength));
        }

        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithInvalidValueCodeAndMessage<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule, string columnName)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrorCodes.InvalidValue)
                .WithMessage(string.Format(ModuleConstants.ValidationErrorMessages[ModuleConstants.ValidationErrorCodes.InvalidValue], columnName));
        }

        public static IRuleBuilderOptions<Property, TProperty> WithInvalidValueCodeAndMessage<TProperty>(this IRuleBuilderOptions<Property, TProperty> rule)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrorCodes.InvalidValue)
                .WithMessage(property => string.Format(ModuleConstants.ValidationErrorMessages[ModuleConstants.ValidationErrorCodes.InvalidValue], property.Name));
        }

        public static IRuleBuilderOptions<PropertyValue, TProperty> WithInvalidValueCodeAndMessage<TProperty>(this IRuleBuilderOptions<PropertyValue, TProperty> rule)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrorCodes.InvalidValue)
                .WithMessage(property => string.Format(ModuleConstants.ValidationErrorMessages[ModuleConstants.ValidationErrorCodes.InvalidValue], property.PropertyName));
        }

        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithNotUniqueValueCodeAndMessage<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule, string columnName)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrorCodes.NotUniqueValue)
                .WithMessage(string.Format(ModuleConstants.ValidationErrorMessages[ModuleConstants.ValidationErrorCodes.NotUniqueValue], columnName));
        }

        public static IRuleBuilderOptions<Property, TProperty> WithNotUniqueMultiValueCodeAndMessage<TProperty>(this IRuleBuilderOptions<Property, TProperty> rule)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrorCodes.NotUniqueValue)
                .WithMessage(property => string.Format(ModuleConstants.ValidationErrorMessages[ModuleConstants.ValidationErrorCodes.NotUniqueMultiValue], property.Name));
        }

        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithNotExistedMainProduct<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrorCodes.MainProductIsNotExists)
                .WithMessage(ModuleConstants.ValidationErrorMessages[ModuleConstants.ValidationErrorCodes.MainProductIsNotExists]);
        }

        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithSelfCycleReference<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrorCodes.CycleSelfReference)
                .WithMessage(ModuleConstants.ValidationErrorMessages[ModuleConstants.ValidationErrorCodes.CycleSelfReference]);
        }

        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithMainProductIsVariation<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrorCodes.MainProductIsVariation)
                .WithMessage(ModuleConstants.ValidationErrorMessages[ModuleConstants.ValidationErrorCodes.MainProductIsVariation]);
        }
    }
}
