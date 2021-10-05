using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Settings;
using catalogCore = VirtoCommerce.CatalogModule.Core;
using coreModuleCore = VirtoCommerce.CoreModule.Core;

namespace VirtoCommerce.CatalogExportImportModule.Data.Validation
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
            var reviewTypes = GetAvailableReviewTypes();

            context.RootContextData[catalogCore.ModuleConstants.Settings.General.EditorialReviewTypes.Name] = reviewTypes;

            var languages = GetAvailableLanguages();

            context.RootContextData[coreModuleCore.ModuleConstants.Settings.General.Languages.Name] = languages;

            var skus = GetExistedSkus(context);

            context.RootContextData[ModuleConstants.ContextDataSkus] = skus;

            var duplicates = GetDescriptionDuplicates(context);

            context.RootContextData[ModuleConstants.DuplicatedImportReview] = duplicates;

            return base.PreValidate(context, result);
        }

        protected virtual string[] GetAvailableReviewTypes()
        {
            var reviewTypesSetting =
                _settingsManager.GetObjectSettingAsync(catalogCore.ModuleConstants.Settings.General.EditorialReviewTypes.Name).GetAwaiter().GetResult();

            var reviewTypes = reviewTypesSetting.AllowedValues.OfType<string>().ToArray();

            return reviewTypes;
        }

        protected virtual string[] GetAvailableLanguages()
        {
            var languagesSetting = _settingsManager.GetObjectSettingAsync(coreModuleCore.ModuleConstants.Settings.General.Languages.Name).GetAwaiter().GetResult();

            var languages = languagesSetting.AllowedValues.OfType<string>().ToArray();

            return languages;
        }

        protected virtual string[] GetExistedSkus(ValidationContext<ImportRecord<CsvEditorialReview>[]> context)
        {
            var skus = context.InstanceToValidate.Select(x => x.Record.ProductSku).ToArray();
            var products = _productSearchService
                .SearchProductsAsync(new ProductSearchCriteria { Skus = skus, Take = skus.Length })
                .GetAwaiter()
                .GetResult();

            return products.Results.Select(x => x.Code).ToArray();
        }

        protected virtual CsvEditorialReview[] GetDescriptionDuplicates(ValidationContext<ImportRecord<CsvEditorialReview>[]> context)
        {
            var importRecords = context.InstanceToValidate.Select(x => x.Record).ToArray();

            var duplicates = importRecords
                .Where(x => !string.IsNullOrEmpty(x.DescriptionId))
                .GroupBy(x => x.DescriptionId)
                .SelectMany(x => x.Take(x.Count() - 1)) // x.Take(x.Count() - 1) means that we want to keep one (last) object as effective value
                .ToArray();

            return duplicates;
        }

        private void AttachValidators()
        {
            RuleForEach(importRecords => importRecords).SetValidator(new ImportDescriptionIsNotDuplicateValidator());
            RuleForEach(importRecords => importRecords).SetValidator(new ImportReviewValidator());
        }
    }
}
