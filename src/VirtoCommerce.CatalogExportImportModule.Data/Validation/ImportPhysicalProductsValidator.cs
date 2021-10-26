using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CoreModule.Core.Package;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogExportImportModule.Data.Validation
{
    public sealed class ImportPhysicalProductsValidator : AbstractValidator<ImportRecord<CsvPhysicalProduct>[]>
    {
        private readonly IPackageTypesService _packageTypesService;
        private readonly ISettingsManager _settingsManager;

        public ImportPhysicalProductsValidator(IPackageTypesService packageTypesService, ISettingsManager settingsManager)
        {
            _packageTypesService = packageTypesService;
            _settingsManager = settingsManager;
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(importRecords => importRecords).SetValidator(new ImportProductsAreNotDuplicatesValidator());
            RuleFor(importRecords => importRecords).CustomAsync(SetContextData).ForEach(x => x.SetValidator(new ImportPhysicalProductValidator()));
            RuleFor(importRecords => importRecords).CustomAsync(SetContextData).ForEach(x => x.SetValidator(new ImportPhysicalProductDescriptionValidator()));
        }

        private async Task SetContextData(ImportRecord<CsvPhysicalProduct>[] records, CustomContext context, CancellationToken cancellationToken)
        {
            context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.AvailablePackageTypes] = await GetAvailablePackageTypesAsync();
            context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.AvailableMeasureUnits] = await GetAvailableMeasureUnits();
            context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.AvailableWeightUnits] = await GetAvailableWeightUnits();
            context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.AvailableTaxTypes] = await GetAvailableTaxTypes();
            context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.AvaibaleLanguages] = await GetAvailableLanguages();
            context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.AvaibaleReviewTypes] = await GetAvailableReviewTypes();
        }

        private async Task<string[]> GetAvailablePackageTypesAsync()
        {
            var packagesTypes = await _packageTypesService.GetAllPackageTypesAsync();
            return packagesTypes.Select(x => x.Name).ToArray();
        }

        private async Task<string[]> GetAvailableMeasureUnits()
        {
            var setting = await _settingsManager.GetObjectSettingAsync(CoreModule.Core.ModuleConstants.Settings.General.MeasureUnits.Name);
            return setting.AllowedValues?.Cast<string>().ToArray() ?? Array.Empty<string>();
        }

        private async Task<string[]> GetAvailableWeightUnits()
        {
            var setting = await _settingsManager.GetObjectSettingAsync(CoreModule.Core.ModuleConstants.Settings.General.WeightUnits.Name);
            return setting.AllowedValues?.Cast<string>().ToArray() ?? Array.Empty<string>();
        }

        private async Task<string[]> GetAvailableTaxTypes()
        {
            var setting = await _settingsManager.GetObjectSettingAsync(CoreModule.Core.ModuleConstants.Settings.General.TaxTypes.Name);
            return setting.AllowedValues?.Cast<string>().ToArray() ?? Array.Empty<string>();
        }

        private async Task<string[]> GetAvailableLanguages()
        {
            var setting = await _settingsManager.GetObjectSettingAsync(CoreModule.Core.ModuleConstants.Settings.General.Languages.Name);
            return setting.AllowedValues.OfType<string>().ToArray() ?? Array.Empty<string>();
        }

        private async Task<string[]> GetAvailableReviewTypes()
        {
            var setting = await _settingsManager.GetObjectSettingAsync(CatalogModule.Core.ModuleConstants.Settings.General.EditorialReviewTypes.Name);
            return setting.AllowedValues.OfType<string>().ToArray() ?? Array.Empty<string>();
        }
    }
}
