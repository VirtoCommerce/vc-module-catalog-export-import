using System.Linq;
using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Data.Helpers;

namespace VirtoCommerce.CatalogExportImportModule.Data.Validation
{
    public sealed class ImportProductIsNotDuplicateValidator : AbstractValidator<ImportRecord<CsvPhysicalProduct>>
    {
        public ImportProductIsNotDuplicateValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecord => importRecord)
                .Must((_, importRecord, context) =>
                {
                    var duplicates = (ImportRecord<CsvPhysicalProduct>[])context.ParentContext.RootContextData[ImportProductsAreNotDuplicatesValidator.Duplicates];
                    return !duplicates.Contains(importRecord);
                })
                .WithErrorCode(ModuleConstants.ValidationErrorCodes.DuplicateError)
                .WithMessage("This product is a duplicate.")
                .WithImportState();
        }
    }
}
