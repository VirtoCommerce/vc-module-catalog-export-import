using FluentValidation;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Data.Helpers;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Validation
{
    public class ImportReviewValidator : AbstractValidator<ImportRecord<CsvEditorialReview>>
    {
        public ImportReviewValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(x => x.Record.LanguageCode)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Language")
                .WithImportState()
                .DependentRules(() =>
                    RuleFor(x => x.Record.LanguageCode)
                        .MaximumLength(64)
                        .WithExceededMaxLengthCodeAndMessage("Language", 64)
                        .WithImportState()
            );

            RuleFor(x => x.Record.ReviewType)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Type")
                .WithImportState()
                .DependentRules(() =>
                    RuleFor(x => x.Record.ReviewType)
                        .MaximumLength(128)
                        .WithExceededMaxLengthCodeAndMessage("Type", 128)
                        .WithImportState()
            );

            RuleFor(x => x.Record.Content)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Description Content")
                .WithImportState();

        }
    }
}
