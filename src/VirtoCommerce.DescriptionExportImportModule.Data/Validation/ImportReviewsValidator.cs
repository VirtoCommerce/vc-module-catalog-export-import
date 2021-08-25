using FluentValidation;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Validation
{
    public class ImportReviewsValidator : AbstractValidator<ImportRecord<CsvEditorialReview>[]>
    {
        public ImportReviewsValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleForEach(importRecords => importRecords).SetValidator(new ImportReviewValidator());
        }
    }
}
