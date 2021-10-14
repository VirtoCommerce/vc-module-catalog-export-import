using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Data.Validation
{
    public class ImportPhysicalProductValidator : AbstractValidator<ImportRecord<CsvPhysicalProduct>[]>
    {
        public ImportPhysicalProductValidator()
        {
            AttachValidators();
        }
        private void AttachValidators()
        {
            RuleFor(importRecords => importRecords).SetValidator(new ImportProductsAreNotDuplicatesValidator());
        }
    }
}
