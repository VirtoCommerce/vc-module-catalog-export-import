using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Settings;
using catalogCore = VirtoCommerce.CatalogModule.Core;
using coreModuleCore = VirtoCommerce.CoreModule.Core;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Validation
{
    public class ImportReviewsValidator : AbstractValidator<ImportRecord<CsvEditorialReview>[]>
    {
        private readonly ISettingsManager _settingsManager;

        public ImportReviewsValidator(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;

            AttachValidators();
        }

        protected override bool PreValidate(ValidationContext<ImportRecord<CsvEditorialReview>[]> context, ValidationResult result)
        {
            var reviewTypesSetting =
                _settingsManager.GetObjectSettingAsync(catalogCore.ModuleConstants.Settings.General.EditorialReviewTypes.Name).GetAwaiter().GetResult();

            var reviewTypes = reviewTypesSetting.AllowedValues.OfType<string>().ToArray();

            context.RootContextData[catalogCore.ModuleConstants.Settings.General.EditorialReviewTypes.Name] =
                reviewTypes;

            var languagesSetting = _settingsManager.GetObjectSettingAsync(coreModuleCore.ModuleConstants.Settings.General.Languages.Name).GetAwaiter().GetResult();

            var languages = languagesSetting.AllowedValues.OfType<string>().ToArray();

            context.RootContextData[coreModuleCore.ModuleConstants.Settings.General.Languages.Name] = languages;

            return base.PreValidate(context, result);
        }

        private void AttachValidators()
        {
            RuleForEach(importRecords => importRecords).SetValidator(new ImportReviewValidator());
        }
    }
}
