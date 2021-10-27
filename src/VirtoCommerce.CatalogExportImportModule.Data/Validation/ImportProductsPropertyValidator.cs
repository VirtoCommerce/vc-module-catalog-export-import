using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Data.Helpers;
using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.CatalogExportImportModule.Data.Validation
{
    public class ImportProductsPropertyValidator : AbstractValidator<Property>
    {
        private readonly ImportRecord<CsvPhysicalProduct> _importRecord;

        internal const string PropertyDictionaryItems = nameof(PropertyDictionaryItems);

        public ImportProductsPropertyValidator(ImportRecord<CsvPhysicalProduct> record)
        {
            _importRecord = record;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(property => property.Values)
                .NotEmpty()
                .WithInvalidValueCodeAndMessage()
                .WithImportState(_importRecord)
                .DependentRules(() =>
                {
                    RuleFor(property => property.Values)
                        .Must(propertyValues => propertyValues.Count == 1)
                        .When(property => !property.Multivalue)
                        .WithInvalidValueCodeAndMessage()
                        .WithImportState(_importRecord)
                        .DependentRules(() =>
                        {
                            When(property => property.Dictionary, () =>
                            {
                                RuleForEach(property => property.Values).ChildRules(childRules =>
                                {
                                    childRules.When(propertyValue => !string.IsNullOrEmpty(propertyValue.Value as string), () =>
                                    {
                                        childRules.RuleFor(propertyValue => propertyValue.ValueId)
                                            .Must((propertyValue, valueId, context) =>
                                            {
                                                var propertyDictionaryItems = (IList<PropertyDictionaryItem>)context.ParentContext.RootContextData[PropertyDictionaryItems];
                                                return propertyDictionaryItems.Any(propertyDictionaryItem =>
                                                    propertyDictionaryItem.PropertyId == propertyValue.PropertyId && propertyDictionaryItem.Id == valueId);
                                            })
                                            .WithInvalidValueCodeAndMessage()
                                            .WithImportState(_importRecord);
                                    });
                                });
                            });

                            RuleForEach(property => property.Values).ChildRules(childRules =>
                            {
                                childRules.When(propertyValue => !string.IsNullOrEmpty(propertyValue.Value as string), () =>
                                {
                                    When(property => property.ValueType == PropertyValueType.ShortText, () =>
                                     {
                                         childRules.RuleFor(propertyValue => propertyValue.Value)
                                             .Must(value => value is string)
                                             .WithInvalidValueCodeAndMessage()
                                             .WithImportState(_importRecord)
                                             .DependentRules(() =>
                                             {
                                                 childRules.RuleFor(propertyValue => propertyValue.Value as string)
                                                     .MaximumLength(512)
                                                     .When(x => x.ValueType == PropertyValueType.ShortText)
                                                     .WithExceededMaxLengthCodeAndMessage(512)
                                                     .WithImportState(_importRecord);
                                             });
                                     });
                                    childRules.RuleFor(propertyValue => propertyValue.Value)
                                        .Must(value => value is string)
                                        .When(propertyValue => propertyValue.ValueType == PropertyValueType.LongText)
                                        .WithInvalidValueCodeAndMessage()
                                        .WithImportState(_importRecord);
                                    childRules.RuleFor(propertyValue => propertyValue.Value)
                                        .Must(value => decimal.TryParse(value as string, out _))
                                        .When(propertyValue => propertyValue.ValueType == PropertyValueType.Number)
                                        .WithInvalidValueCodeAndMessage()
                                        .WithImportState(_importRecord);
                                    childRules.RuleFor(propertyValue => propertyValue.Value)
                                        .Must(value => int.TryParse(value as string, out _))
                                        .When(propertyValue => propertyValue.ValueType == PropertyValueType.Integer)
                                        .WithInvalidValueCodeAndMessage()
                                        .WithImportState(_importRecord);
                                    childRules.RuleFor(propertyValue => propertyValue.Value)
                                        .Must(value => bool.TryParse(value as string, out _))
                                        .When(propertyValue => propertyValue.ValueType == PropertyValueType.Boolean)
                                        .WithInvalidValueCodeAndMessage()
                                        .WithImportState(_importRecord);
                                    childRules.RuleFor(propertyValue => propertyValue.Value)
                                        .Must(value => DateTime.TryParse(value as string, out _))
                                        .When(propertyValue => propertyValue.ValueType == PropertyValueType.DateTime)
                                        .WithInvalidValueCodeAndMessage()
                                        .WithImportState(_importRecord);
                                });
                            });
                        });
                });
        }
    }
}
