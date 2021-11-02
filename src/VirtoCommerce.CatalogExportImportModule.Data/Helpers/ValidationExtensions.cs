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
                .WithErrorCode(ModuleConstants.ValidationErrors.MissingRequiredValues)
                .WithMessage(string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.MissingRequiredValues], columnName));
        }

        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithExceededMaxLengthCodeAndMessage<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule, string columnName, int maxLength)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength)
                .WithMessage(string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.ExceedingMaxLength], columnName, maxLength));
        }

        public static IRuleBuilderOptions<PropertyValue, TProperty> WithExceededMaxLengthCodeAndMessage<TProperty>(this IRuleBuilderOptions<PropertyValue, TProperty> rule, int maxLength)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrors.ExceedingMaxLength)
                .WithMessage(dynamicPropertyValue => string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.ExceedingMaxLength], dynamicPropertyValue.PropertyName, maxLength));
        }

        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithInvalidValueCodeAndMessage<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule, string columnName)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrors.InvalidValue)
                .WithMessage(string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.InvalidValue], columnName));
        }

        public static IRuleBuilderOptions<Property, TProperty> WithInvalidValueCodeAndMessage<TProperty>(this IRuleBuilderOptions<Property, TProperty> rule)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrors.InvalidValue)
                .WithMessage(property => string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.InvalidValue], property.Name));
        }

        public static IRuleBuilderOptions<PropertyValue, TProperty> WithInvalidValueCodeAndMessage<TProperty>(this IRuleBuilderOptions<PropertyValue, TProperty> rule)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrors.InvalidValue)
                .WithMessage(property => string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.InvalidValue], property.PropertyName));
        }

        public static IRuleBuilderOptions<ImportRecord<T>, TProperty> WithNotUniqueValueCodeAndMessage<T, TProperty>(this IRuleBuilderOptions<ImportRecord<T>, TProperty> rule, string columnName)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrors.NotUniqueValue)
                .WithMessage(string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.NotUniqueValue], columnName));
        }

        public static IRuleBuilderOptions<Property, TProperty> WithNotUniqueMultiValueCodeAndMessage<TProperty>(this IRuleBuilderOptions<Property, TProperty> rule)
        {
            return rule
                .WithErrorCode(ModuleConstants.ValidationErrors.NotUniqueValue)
                .WithMessage(property => string.Format(ModuleConstants.ValidationMessages[ModuleConstants.ValidationErrors.NotUniqueMultiValue], property.Name));
        }
    }
}
