using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.DescriptionExportImportModule.Core;
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
            var reviewTypesSetting =
                _settingsManager.GetObjectSettingAsync(catalogCore.ModuleConstants.Settings.General.EditorialReviewTypes.Name).GetAwaiter().GetResult();

            var reviewTypes = reviewTypesSetting.AllowedValues.OfType<string>().ToArray();

            context.RootContextData[catalogCore.ModuleConstants.Settings.General.EditorialReviewTypes.Name] =
                reviewTypes;

            var languagesSetting = _settingsManager.GetObjectSettingAsync(coreModuleCore.ModuleConstants.Settings.General.Languages.Name).GetAwaiter().GetResult();

            var languages = languagesSetting.AllowedValues.OfType<string>().ToArray();

            context.RootContextData[coreModuleCore.ModuleConstants.Settings.General.Languages.Name] = languages;

            var skus = context.InstanceToValidate.Select(x => x.Record.ProductSku).ToArray();
            var products = _productSearchService
                .SearchProductsAsync(new ProductSearchCriteria { Skus = skus, Take = skus.Length })
                .GetAwaiter()
                .GetResult();

            context.RootContextData[ModuleConstants.ContextDataSkus] = products.Results.Select(x => x.Code).ToArray();

            return base.PreValidate(context, result);
        }

        private void AttachValidators()
        {
            RuleForEach(importRecords => importRecords).SetValidator(new ImportReviewValidator(_productSearchService));
        }
    }
}
