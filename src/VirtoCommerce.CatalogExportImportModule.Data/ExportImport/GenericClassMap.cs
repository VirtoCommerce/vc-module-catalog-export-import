using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Data.ExportImport
{
    public sealed class GenericClassMap<T> : ClassMap<T>
    {
        public GenericClassMap(IList<Property> dynamicProperties, Dictionary<string, IList<PropertyDictionaryItem>> propertyDictionaryItems = null)
        {
            AutoMap(new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" });

            var typeHasDynamicProperties = ClassType.GetInterfaces().Contains(typeof(IHasProperties));

            if (!dynamicProperties.IsNullOrEmpty() && typeHasDynamicProperties)
            {
                AddDynamicPropertyColumnDefinitionAndWritingMap(dynamicProperties);

                AddDynamicPropertyReadingMap(dynamicProperties, propertyDictionaryItems);
            }
        }

        private void AddDynamicPropertyColumnDefinitionAndWritingMap(IList<Property> dynamicProperties)
        {
            var currentColumnIndex = MemberMaps.Count;

            var dynamicPropertiesPropertyInfo = ClassType.GetProperty(nameof(IHasProperties.Properties));

            // Exporting multiple csv fields from the same property (which is a collection)
            foreach (var property in dynamicProperties)
            {
                // create CsvPropertyMap manually, because this.Map(x =>...) does not allow
                // to export multiple entries for the same property
                var propertyColumnDefinitionAndWriteMap = MemberMap.CreateGeneric(ClassType, dynamicPropertiesPropertyInfo);
                propertyColumnDefinitionAndWriteMap.Name(property.Name);
                propertyColumnDefinitionAndWriteMap.Data.IsOptional = true;
                propertyColumnDefinitionAndWriteMap.Data.Index = currentColumnIndex++;

                // create custom converter instance which will get the required record from the collection
                propertyColumnDefinitionAndWriteMap.UsingExpression<ICollection<Property>>(null, dynamicObjectProperties =>
                {
                    var dynamicObjectProperty = dynamicObjectProperties.FirstOrDefault(x => x.Name == property.Name && x.Values.Any());
                    var dynamicObjectPropertyValues = Array.Empty<string>();

                    if (dynamicObjectProperty != null)
                    {
                        if (dynamicObjectProperty.Dictionary)
                        {
                            dynamicObjectPropertyValues = dynamicObjectProperty.Values?
                                .Where(x => x.Value != null)
                                .Select(x => x.Value.ToString())
                                .Distinct()
                                .ToArray();
                        }
                        else
                        {
                            dynamicObjectPropertyValues = dynamicObjectProperty.Values?
                                .Where(x => x.Value != null)
                                .Select(x => x.Value.ToString())
                                .ToArray();
                        }
                    }

                    return string.Join(", ", dynamicObjectPropertyValues);
                });

                MemberMaps.Add(propertyColumnDefinitionAndWriteMap);
            }
        }

        private void AddDynamicPropertyReadingMap(IList<Property> dynamicProperties, Dictionary<string, IList<PropertyDictionaryItem>> propertyDictionaryItems)
        {
            var currentColumnIndex = MemberMaps.Count;

            var dynamicPropertiesPropertyInfo = ClassType.GetProperty(nameof(IHasProperties.Properties));

            var propertyReadingMap = MemberMap.CreateGeneric(ClassType, dynamicPropertiesPropertyInfo);
            propertyReadingMap.Data.ReadingConvertExpression =
                (Expression<Func<IReaderRow, object>>)(row => dynamicProperties
                   .Select(property =>
                       !string.IsNullOrEmpty(row.GetField<string>(property.Name))
                           ? new Property()
                           {
                               Id = property.Id,
                               Name = property.Name,
                               DisplayNames = property.DisplayNames,
                               //Displa = property.DisplayOrder,
                               //Description = property.Description,
                               Multivalue = property.Multivalue,
                               Dictionary = property.Dictionary,
                               Multilanguage = property.Multilanguage,
                               Required = property.Required,
                               ValueType = property.ValueType,
                               Values = ToDynamicPropertyValues(property, propertyDictionaryItems, row.GetField<string>(property.Name))
                           }
                           : null)
                   .Where(x => x != null)
                   .ToList());
            propertyReadingMap.UsingExpression<ICollection<Property>>(null, null);
            propertyReadingMap.Ignore(true);
            propertyReadingMap.Data.IsOptional = true;
            propertyReadingMap.Data.Index = currentColumnIndex + 1;
            MemberMaps.Add(propertyReadingMap);
        }

        private IList<PropertyValue> ToDynamicPropertyValues(Property property, Dictionary<string, IList<PropertyDictionaryItem>> propertyDictionaryItems, string values)
        {
            return property.Multivalue
                ? ToDynamicPropertyMultiValue(property, propertyDictionaryItems, values)
                : new List<PropertyValue> { ToDynamicPropertyValue(property, propertyDictionaryItems, values) };
        }

        private PropertyValue ToDynamicPropertyValue(Property property, Dictionary<string, IList<PropertyDictionaryItem>> propertyDictionaryItems, string value)
        {
            return new PropertyValue
            {
                PropertyName = property.Name,
                PropertyId = property.Id,
                Value = value,
                ValueType = property.ValueType,
                ValueId = property.Dictionary && propertyDictionaryItems[property.Id]
                    .Any(dictionaryItem => dictionaryItem.Alias == value)
                    ? propertyDictionaryItems[property.Id].FirstOrDefault(dictionaryItem => dictionaryItem.Alias == value)?.Id
                    : null
            };
        }

        private IList<PropertyValue> ToDynamicPropertyMultiValue(Property property, Dictionary<string, IList<PropertyDictionaryItem>> propertyDictionaryItems, string values)
        {
            var parsedValues = values.Split(',').Select(value => value.Trim()).ToList();
            var convertedValues = parsedValues.Select(value => ToDynamicPropertyValue(property, propertyDictionaryItems, value));
            return convertedValues.ToList();
        }
    }
}


