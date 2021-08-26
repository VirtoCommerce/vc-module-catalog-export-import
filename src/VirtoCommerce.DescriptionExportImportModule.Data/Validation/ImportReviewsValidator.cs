using System;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Settings;
using catalogCore = VirtoCommerce.CatalogModule.Core;
using coreModuleCore = VirtoCommerce.CoreModule.Core;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Validation
{
    public class ImportReviewsValidator : AbstractValidator<ImportRecord<CsvEditorialReview>[]>
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IProductSearchService _productSearchService;

        public ImportReviewsValidator(ISettingsManager settingsManager, IProductSearchService productSearchService)
        {
            _settingsManager = settingsManager;
            _productSearchService = productSearchService;

            AttachValidators();
        }

        protected override bool PreValidate(ValidationContext<ImportRecord<CsvEditorialReview>[]> context, ValidationResult result)
        {
            var reviewTypes = _settingsManager.GetValue(catalogCore.ModuleConstants.Settings.General.EditorialReviewTypes.Name,
                Array.Empty<string>());

            context.RootContextData[catalogCore.ModuleConstants.Settings.General.EditorialReviewTypes.Name] =
                reviewTypes;

            var languages = _settingsManager.GetValue(coreModuleCore.ModuleConstants.Settings.General.Languages.Name, Array.Empty<string>());

            context.RootContextData[coreModuleCore.ModuleConstants.Settings.General.Languages.Name] = languages;

            return base.PreValidate(context, result);
        }

        private void AttachValidators()
        {
            RuleForEach(importRecords => importRecords).SetValidator(new ImportReviewValidator());
        }
    }
}
