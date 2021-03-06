using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CoreModule.Core.Package;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogExportImportModule.Data.Validation
{
    public sealed class ImportPhysicalProductsValidator : AbstractValidator<ImportRecord<CsvPhysicalProduct>[]>
    {
        private readonly IPackageTypesService _packageTypesService;
        private readonly ISettingsManager _settingsManager;
        private readonly IProductEditorialReviewService _editorialReviewService;
        private readonly IPropertyDictionaryItemSearchService _propertyDictionaryItemSearchService;
        private readonly IItemService _itemService;
        private readonly IProductSearchService _productSearchService;

        public ImportPhysicalProductsValidator(
        IPackageTypesService packageTypesService,
        ISettingsManager settingsManager,
        IProductEditorialReviewService editorialReviewService,
        IPropertyDictionaryItemSearchService propertyDictionaryItemSearchService,
        IItemService itemService,
        IProductSearchService productSearchService)
        {
            _packageTypesService = packageTypesService;
            _settingsManager = settingsManager;
            _editorialReviewService = editorialReviewService;
            _propertyDictionaryItemSearchService = propertyDictionaryItemSearchService;
            _itemService = itemService;
            _productSearchService = productSearchService;

            AttachValidators();
        }


        private void AttachValidators()
        {
            RuleFor(importRecords => importRecords).SetValidator(new ImportProductsAreNotDuplicatesValidator());
            RuleForEach(importRecords => importRecords).SetValidator(new ImportProductCategoryValidator());
            RuleFor(importRecords => importRecords).CustomAsync(SetContextData).ForEach(x => x.SetValidator(new ImportPhysicalProductValidator()));
            RuleFor(importRecords => importRecords).CustomAsync(SetContextData).ForEach(x => x.SetValidator(new ImportPhysicalProductSkuValidator()));
            RuleFor(importRecords => importRecords).CustomAsync(SetContextData).ForEach(x => x.SetValidator(new ImportPhysicalProductDescriptionValidator()));
        }

        private async Task SetContextData(ImportRecord<CsvPhysicalProduct>[] records, ValidationContext<ImportRecord<CsvPhysicalProduct>[]> context, CancellationToken cancellationToken)
        {
            var catalogId =
                context.RootContextData[ModuleConstants.ValidationContextData.CatalogId] as string;

            var importedReviewIds = records.Select(x => x.Record.DescriptionId)
                .Distinct()
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            var propertyIds = records.SelectMany(x => x.Record.Properties)
                .Where(x => x.Dictionary).Select(x => x.Id).Distinct().ToArray();

            var skus = records.Select(x => x.Record.ProductSku).Where(x => !string.IsNullOrEmpty(x)).ToArray();

            context.RootContextData[ModuleConstants.ValidationContextData.AvailablePackageTypes] = await GetAvailablePackageTypesAsync();
            context.RootContextData[ModuleConstants.ValidationContextData.AvailableMeasureUnits] = await GetAvailableMeasureUnits();
            context.RootContextData[ModuleConstants.ValidationContextData.AvailableWeightUnits] = await GetAvailableWeightUnits();
            context.RootContextData[ModuleConstants.ValidationContextData.AvailableTaxTypes] = await GetAvailableTaxTypes();
            context.RootContextData[ModuleConstants.ValidationContextData.AvailableLanguages] = await GetAvailableLanguages();
            context.RootContextData[ModuleConstants.ValidationContextData.AvailableReviewTypes] = await GetAvailableReviewTypes();
            context.RootContextData[ModuleConstants.ValidationContextData.ExistedReviews] = (await _editorialReviewService.GetByIdsAsync(importedReviewIds)).OfType<ExtendedEditorialReview>().ToArray();
            context.RootContextData[ImportProductsPropertyValidator.PropertyDictionaryItems] =
                await GetPropertyDictionaryItems(propertyIds);
                
            context.RootContextData[ModuleConstants.ValidationContextData.ExistingMainProducts] = await GetExistingMainProductsAsync(records);
            
            context.RootContextData[ModuleConstants.ValidationContextData.ExistedProductsWithSameSku] =
                (await _productSearchService.SearchProductsAsync(new ProductSearchCriteria() { CatalogId = catalogId, Skus = skus, Take = ModuleConstants.Settings.PageSize })).Results.ToArray();
        }

        private async Task<PropertyDictionaryItem[]> GetPropertyDictionaryItems(string[] propertyIds)
        {
            if (propertyIds.IsNullOrEmpty())
            {
                return Array.Empty<PropertyDictionaryItem>();

            }

            var dynamicPropertyDictionaryItemsSearchResult =
                    await _propertyDictionaryItemSearchService.SearchAsync(new PropertyDictionaryItemSearchCriteria { PropertyIds = propertyIds });

            return dynamicPropertyDictionaryItemsSearchResult.Results.ToArray();
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

        private async Task<CatalogProduct[]> GetExistingMainProductsAsync(ImportRecord<CsvPhysicalProduct>[] records)
        {
            var productIds = records.Select(x => x.Record?.MainProductId).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray();

            var products = await _itemService.GetByIdsAsync(productIds, ItemResponseGroup.ItemInfo.ToString());

            return products;
        }
    }
}
