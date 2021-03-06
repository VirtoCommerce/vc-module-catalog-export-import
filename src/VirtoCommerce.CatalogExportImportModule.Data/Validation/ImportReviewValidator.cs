using System;
using System.Linq;
using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Data.Helpers;
using catalogCore = VirtoCommerce.CatalogModule.Core;
using coreModuleCore = VirtoCommerce.CoreModule.Core;

namespace VirtoCommerce.CatalogExportImportModule.Data.Validation
{
    public class ImportReviewValidator : AbstractValidator<ImportRecord<CsvEditorialReview>>
    {
        public ImportReviewValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(x => x.Record.Language)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Language")
                .WithImportState()
                .DependentRules(() =>
                        RuleFor(x => x.Record.Language)
                            .Must((_, language, context) =>
                            {
                                var languages = (string[])context.RootContextData[coreModuleCore.ModuleConstants.Settings.General.Languages.Name];
                                return languages.Contains(language, StringComparer.InvariantCultureIgnoreCase);
                            })
                            .WithInvalidValueCodeAndMessage("Language")
                            .WithImportState()
                );

            RuleFor(x => x.Record.Type)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Type")
                .WithImportState()
                .DependentRules(() =>
                    RuleFor(x => x.Record.Type)
                        .Must((_, reviewType, context) =>
                        {
                            var reviewTypes = (string[])context.RootContextData[catalogCore.ModuleConstants.Settings.General.EditorialReviewTypes.Name];
                            return reviewTypes.Contains(reviewType, StringComparer.InvariantCultureIgnoreCase);
                        })
                        .WithInvalidValueCodeAndMessage("Type")
                        .WithImportState()
                );

            RuleFor(x => x.Record.DescriptionContent)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Description Content")
                .WithImportState();

            When(x => string.IsNullOrEmpty(x.Record.DescriptionId),
                () =>
                    RuleFor(x => x.Record.ProductSku)
                        .NotEmpty()
                        .WithMissingRequiredValueCodeAndMessage("Product SKU")
                        .WithImportState()
            );

            When(x => !string.IsNullOrEmpty(x.Record.ProductSku),
            () =>
                RuleFor(x => x.Record.ProductSku)
                    .Must((_, sku, context) =>
                    {
                        var skus = (string[])context.RootContextData[ModuleConstants.ValidationContextData.Skus];
                        return skus.Contains(sku);
                    })
                    .WithErrorCode("sku-is-not-exist")
                    .WithMessage("Product with such SKU does not exist.")
                    .WithImportState()
                );

        }
    }
}
