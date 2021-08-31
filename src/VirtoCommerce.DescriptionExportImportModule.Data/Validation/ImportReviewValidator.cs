using System;
using System.Linq;
using FluentValidation;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Data.Helpers;
using catalogCore = VirtoCommerce.CatalogModule.Core;
using coreModuleCore = VirtoCommerce.CoreModule.Core;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Validation
{
    public class ImportReviewValidator : AbstractValidator<ImportRecord<CsvEditorialReview>>
    {
        private readonly IProductSearchService _productSearchService;

        public ImportReviewValidator(IProductSearchService productSearchService)
        {
            _productSearchService = productSearchService;

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
                                var languages = (string[])context.ParentContext.RootContextData[
                                        coreModuleCore.ModuleConstants.Settings.General.Languages.Name];
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
                            var reviewTypes = (string[])context.ParentContext.RootContextData[
                                catalogCore.ModuleConstants.Settings.General.EditorialReviewTypes.Name];
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
                    .MustAsync(async (sku, _) =>
                    {
                        var productSearchResult = await _productSearchService.SearchProductsAsync(new ProductSearchCriteria { Take = 0, Skus = new[] { sku }, });
                        return productSearchResult.TotalCount > 0;
                    })
                    .WithErrorCode("sku-is-not-exist")
                    .WithMessage("Product SKU is not exist")
                    .WithImportState()
                );

        }
    }
}
