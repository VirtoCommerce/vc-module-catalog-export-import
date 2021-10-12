using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogModule.Core.Search;

namespace VirtoCommerce.CatalogExportImportModule.Data.Validation
{
    public class ImportPhysicalProductValidator : AbstractValidator<ImportRecord<CsvPhysicalProduct>[]>
    {
        
        private readonly IProductSearchService _productSearchService;

        public ImportPhysicalProductValidator(IProductSearchService productSearchService)
        {
            _productSearchService = productSearchService;

            AttachValidators();
        }

        protected override bool PreValidate(ValidationContext<ImportRecord<CsvPhysicalProduct>[]> context, ValidationResult result)
        {
            var reviewTypes = GetAvailableReviewTypes();

            context.RootContextData[catalogCore.ModuleConstants.Settings.General.EditorialReviewTypes.Name] = reviewTypes;

            var languages = GetAvailableLanguages();

            context.RootContextData[coreModuleCore.ModuleConstants.Settings.General.Languages.Name] = languages;

            var skus = GetExistedSkus(context);

            context.RootContextData[ModuleConstants.ContextDataSkus] = skus;

            var duplicates = GetDescriptionDuplicates(context);

            context.RootContextData[ModuleConstants.DuplicatedImportReview] = duplicates;

            return base.PreValidate(context, result);
        

        private void AttachValidators()
        {
            RuleForEach(importRecords => importRecords).SetValidator(new ImportProductsAreNotDuplicatesValidator());
        }
    }
}
