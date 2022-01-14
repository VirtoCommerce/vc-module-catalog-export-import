using System;
using System.Linq;
using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Data.Helpers;

namespace VirtoCommerce.CatalogExportImportModule.Data.Validation
{
    public class ImportPhysicalProductDescriptionValidator : AbstractValidator<ImportRecord<CsvPhysicalProduct>>
    {
        public ImportPhysicalProductDescriptionValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            When(x => !string.IsNullOrEmpty(x.Record.DescriptionId),
                () =>
                RuleFor(x => x.Record.DescriptionId)
                    .Must((record, id, context) =>
                {
                    var existedReviews = (ExtendedEditorialReview[])context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.ExistedReviews];
                    var existedReview = existedReviews.FirstOrDefault(x => x.Id == id);

                    if (existedReview == null)
                    {
                        return true;
                    }

                    var result = existedReview.ItemId == record.Record.ProductId;
                    return result;
                })
                    .WithErrorCode(ModuleConstants.ValidationErrorCodes.ReviewExistsInSystem)
                    .WithMessage("Description exists in the system related to other product")
                    .WithImportState()
                   );

            When(x => new[]
            {
                x.Record.Description, x.Record.DescriptionLanguage, x.Record.DescriptionType
            }.Any(field => !string.IsNullOrEmpty(field)), () =>
            {
                RuleFor(x => x.Record.Description)
                    .NotEmpty()
                    .WithMissingRequiredValueCodeAndMessage("Description")
                    .WithImportState();

                RuleFor(x => x.Record.DescriptionLanguage)
                    .NotEmpty()
                    .WithMissingRequiredValueCodeAndMessage("Description Language")
                    .WithImportState()
                    .DependentRules(() =>
                    {
                        RuleFor(x => x.Record.DescriptionLanguage)
                            .MaximumLength(64)
                            .WithExceededMaxLengthCodeAndMessage("Description Language", 64)
                            .WithImportState();
                        RuleFor(x => x.Record.DescriptionLanguage)
                            .Must((_, language, context) =>
                            {
                                var languages = (string[])context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.AvailableLanguages];
                                return languages.Contains(language, StringComparer.InvariantCultureIgnoreCase);
                            })
                            .WithInvalidValueCodeAndMessage("Description Language")
                            .WithImportState();
                    });

                RuleFor(x => x.Record.DescriptionType)
                    .NotEmpty()
                    .WithMissingRequiredValueCodeAndMessage("Description Type")
                    .WithImportState()
                    .DependentRules(() =>
                    {
                        RuleFor(x => x.Record.DescriptionType)
                            .MaximumLength(128)
                            .WithExceededMaxLengthCodeAndMessage("Description Type", 128)
                            .WithImportState();
                        RuleFor(x => x.Record.DescriptionType)
                            .Must((_, reviewType, context) =>
                            {
                                var reviewTypes = (string[])context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.AvailableReviewTypes];
                                return reviewTypes.Contains(reviewType, StringComparer.InvariantCultureIgnoreCase);
                            })
                            .WithInvalidValueCodeAndMessage("Description Type")
                            .WithImportState();
                    });
            });
        }
    }
}
