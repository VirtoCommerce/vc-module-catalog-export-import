using System.Linq;
using FluentValidation;
using VirtoCommerce.DescriptionExportImportModule.Core;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Data.Helpers;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Validation
{
    public class ImportEntityIsNotDuplicateValidator : AbstractValidator<ImportRecord<CsvEditorialReview>>
    {
        public ImportEntityIsNotDuplicateValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecord => importRecord)
                .Must((_, importRecord, context) =>
                {
                    var duplicates = (CsvEditorialReview[])context.ParentContext.RootContextData[ModuleConstants.DuplicatedImportReview];

                    return !duplicates.Contains(importRecord.Record);
                })
                .WithErrorCode(ModuleConstants.ValidationErrors.DuplicateError)
                .WithMessage("This description is a duplicate.")
                .WithImportState();
        }
    }
}
