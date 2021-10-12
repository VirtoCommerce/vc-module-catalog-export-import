using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Data.Validation
{
    public sealed class ImportProductsAreNotDuplicatesValidator : AbstractValidator<ImportRecord<CsvPhysicalProduct>[]>
    {
        internal const string Duplicates = nameof(Duplicates);

        public ImportProductsAreNotDuplicatesValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecord => importRecord)
                .ForEach(rule => rule.SetValidator(_ => new ImportProductIsNotDuplicateValidator()));
        }

        protected override bool PreValidate(ValidationContext<ImportRecord<CsvPhysicalProduct>[]> context, ValidationResult result)
        {
            var importRecords = context.InstanceToValidate.ToArray();

            var duplicates = GetDuplicates(importRecords, context);

            context.RootContextData[Duplicates] = duplicates;

            return base.PreValidate(context, result);
        }

        private ImportRecord<CsvPhysicalProduct>[] GetDuplicates(ImportRecord<CsvPhysicalProduct>[] importRecords, ValidationContext<ImportRecord<CsvPhysicalProduct>[]> context)
        {
            var duplicatesById = importRecords.Where(importRecord => !string.IsNullOrEmpty(importRecord.Record.ProductId))
                .GroupBy(importRecord => importRecord.Record.ProductId)
                .SelectMany(group => group.Take(group.Count() - 1))
                .ToArray();

            var duplicatesByOuterId = importRecords.Where(importRecord => !string.IsNullOrEmpty(importRecord.Record.ProductOuterId))
                .GroupBy(importRecord => importRecord.Record.ProductOuterId)
                .SelectMany(group => group.Take(group.Count() - 1))
                .ToArray();

            return duplicatesById.Concat(duplicatesByOuterId).Distinct().ToArray();
        }
    }
}
