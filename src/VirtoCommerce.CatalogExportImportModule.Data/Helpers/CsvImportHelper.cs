using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper.Configuration.Attributes;

namespace VirtoCommerce.CatalogExportImportModule.Data.Helpers
{
    public static class CsvImportHelper
    {
        public static string[] GetImportCustomerRequiredColumns<T>()
        {
            var requiredColumns = typeof(T).GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(RequiredAttribute)))
                .Select(p =>
                    ((NameAttribute)Attribute.GetCustomAttribute(p, typeof(NameAttribute)))?.Names.First() ??
                    p.Name).ToArray();

            return requiredColumns;
        }

        public static IList<string> SplitGeoPointMultivalueString(string values)
        {
            const string splitBySecondCommaPattern = @"([^,]*,[^,]*),";
            var parsedValues = Regex.Split(values, splitBySecondCommaPattern).Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim()).ToList();
            return parsedValues;
        }
    }
}
