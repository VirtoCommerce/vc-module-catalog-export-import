using System.Linq;
using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Data.Helpers;

namespace VirtoCommerce.CatalogExportImportModule.Data.Validation
{
    public class ImportDescriptionIsNotDuplicateValidator : AbstractValidator<ImportRecord<CsvEditorialReview>>
    {
        public ImportDescriptionIsNotDuplicateValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecord => importRecord)
                .Must((_, importRecord, context) =>
                {
                    var duplicates = (CsvEditorialReview[])context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.DuplicatedImportReview];

                    return !duplicates.Contains(importRecord.Record);
                })
                .WithErrorCode(ModuleConstants.ValidationErrorCodes.DuplicateError)
                .WithMessage("This description is a duplicate.")
                .WithImportState();
        }
    }
}
