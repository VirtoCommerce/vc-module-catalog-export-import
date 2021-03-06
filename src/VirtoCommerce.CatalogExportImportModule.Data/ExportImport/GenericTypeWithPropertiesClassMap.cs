using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Data.Helpers;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Data.ExportImport
{
    public sealed class GenericTypeWithPropertiesClassMap<T> : ClassMap<T> where T : IHasProperties
    {
        public GenericTypeWithPropertiesClassMap(Property[] properties, Dictionary<string, PropertyDictionaryItem[]> propertyDictionaryItems = null)
        {
            AutoMap(new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" });

            var typeHasProperties = ClassType.GetInterfaces().Contains(typeof(IHasProperties));

            if (!properties.IsNullOrEmpty() && typeHasProperties)
            {
                AddPropertiesWritingMap(properties);

                AddPropertiesReadingMap(properties, propertyDictionaryItems);
            }
        }

        private void AddPropertiesWritingMap(Property[] exportedProperties)
        {
            var currentColumnIndex = MemberMaps.Count;

            var propertiesPropertyInfo = ClassType.GetProperty(nameof(IHasProperties.Properties));

            // Exporting multiple csv fields from the same property (which is a collection)
            foreach (var exportedProperty in exportedProperties)
            {
                // create CsvPropertyMap manually, because this.Map(x =>...) does not allow
                // to export multiple entries for the same property
                var propertyColumnDefinitionAndWriteMap = MemberMap.CreateGeneric(ClassType, propertiesPropertyInfo);
                propertyColumnDefinitionAndWriteMap.Name(exportedProperty.Name);
                propertyColumnDefinitionAndWriteMap.Data.IsOptional = true;
                propertyColumnDefinitionAndWriteMap.Data.Index = currentColumnIndex++;

                // create custom converter instance which will get the required record from the collection
                ConvertToString<T> func = args =>
                {

                    var valueProperty = args.Value.Properties.FirstOrDefault(x => x.Name == exportedProperty.Name && x.Values.Any());
                    var valuePropertyValues = Array.Empty<string>();

                    if (valueProperty != null)
                    {
                        valuePropertyValues = valueProperty.Values?
                            .Where(x => x.Value != null)
                            .Select(x => string.Format(CultureInfo.InvariantCulture, "{0}", x.Value))
                            .Distinct()
                            .ToArray();
                    }

                    var result = string.Join(", ", valuePropertyValues);

                    return result;
                };

                propertyColumnDefinitionAndWriteMap.Data.WritingConvertExpression =
                    (Expression<ConvertToString<T>>)(args => func(args));


                MemberMaps.Add(propertyColumnDefinitionAndWriteMap);
            }
        }

        private void AddPropertiesReadingMap(Property[] properties, Dictionary<string, PropertyDictionaryItem[]> propertyDictionaryItems)
        {
            var currentColumnIndex = MemberMaps.Count;

            var propertiesPropertyInfo = ClassType.GetProperty(nameof(IHasProperties.Properties));

            var propertyReadingMap = MemberMap.CreateGeneric(ClassType, propertiesPropertyInfo);

            ConvertFromString<IList<Property>> func = args =>
            {
                var row = args.Row;

                var propertiesFromFile = properties.Where(x => row.HeaderRecord.Contains(x.Name)).ToArray();

                var result = propertiesFromFile
                    .Select(property =>
                        !string.IsNullOrEmpty(row.GetField<string>(property.Name))
                            ? new Property()
                            {
                                Id = property.Id,
                                Name = property.Name,
                                DisplayNames = property.DisplayNames,
                                Multivalue = property.Multivalue,
                                Dictionary = property.Dictionary,
                                Multilanguage = property.Multilanguage,
                                Required = property.Required,
                                ValueType = property.ValueType,
                                Values = ToPropertyValues(property, propertyDictionaryItems, row.GetField<string>(property.Name))
                            }
                            : null)
                    .Where(x => x != null)
                    .ToList();

                return result;
            };

            propertyReadingMap.Data.ReadingConvertExpression =
                (Expression<ConvertFromString<IList<Property>>>)(args => func(args));

            propertyReadingMap.Ignore(true);
            propertyReadingMap.Data.IsOptional = true;
            propertyReadingMap.Data.Index = currentColumnIndex + 1;

            MemberMaps.Add(propertyReadingMap);
        }

        private IList<PropertyValue> ToPropertyValues(Property property, Dictionary<string, PropertyDictionaryItem[]> propertyDictionaryItems, string values)
        {
            return property.Multivalue
                ? ToPropertyMultiValue(property, propertyDictionaryItems, values)
                : new List<PropertyValue> { ToPropertyValue(property, propertyDictionaryItems, values) };
        }

        private PropertyValue ToPropertyValue(Property property, Dictionary<string, PropertyDictionaryItem[]> propertyDictionaryItems, string value)
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

        private IList<PropertyValue> ToPropertyMultiValue(Property property, Dictionary<string, PropertyDictionaryItem[]> propertyDictionaryItems, string values)
        {
            IList<string> parsedValues;

            if (property.ValueType == PropertyValueType.GeoPoint)
            {
                parsedValues = CsvImportHelper.SplitGeoPointMultivalueString(values);
            }
            else
            {
                parsedValues = values.Split(',').Select(value => value.Trim()).ToList();
            }

            var convertedValues = parsedValues.Select(value => ToPropertyValue(property, propertyDictionaryItems, value));
            return convertedValues.ToList();
        }


    }
}


