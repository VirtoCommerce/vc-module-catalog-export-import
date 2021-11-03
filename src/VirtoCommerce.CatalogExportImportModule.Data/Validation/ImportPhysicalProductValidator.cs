using System.Linq;
using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using static VirtoCommerce.CatalogExportImportModule.Data.Helpers.ValidationExtensions;

namespace VirtoCommerce.CatalogExportImportModule.Data.Validation
{
    public sealed class ImportPhysicalProductValidator : AbstractValidator<ImportRecord<CsvPhysicalProduct>>
    {
        private static readonly char[] ProductSkuIllegalCharacters = { '$', '+', ';', '=', '%', '{', '}', '[', ']', '|', '@', '~', '!', '^', '*', '&', '(', ')', '<', '>' };

        public ImportPhysicalProductValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            // validate that id and outer id are matching
            RuleFor(record => record)
                .Configure(rule => rule.CascadeMode = CascadeMode.StopOnFirstFailure)
                .Must((record, _, context) =>
                {
                    var existedProducts =
                        (CatalogProduct[])context.ParentContext.RootContextData[
                            ModuleConstants.ValidationContextData.ExistedProducts];

                    var productById = existedProducts.FirstOrDefault(p =>
                        record.Record.ProductId.EqualsInvariant(p.Id));

                    var productByOuterId = existedProducts.FirstOrDefault(p =>
                        record.Record.ProductId.EqualsInvariant(p.OuterId));

                    if (productById == null && productByOuterId == null)
                    {
                        return true;
                    }

                    return productById == productByOuterId;
                })
                .When(record => !string.IsNullOrEmpty(record.Record.ProductId) &&
                                !string.IsNullOrEmpty(record.Record.ProductOuterId))
                .WithMessage("Another product with the same Outer Id exists in the system.")
                .WithImportState();

            // validate catalog of product matching to request
            RuleFor(record => record)
                .Must((record, _, context) =>
                {
                    var result = true;

                    var catalogId =
                        context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.CatalogId] as string;

                    var existedProducts =
                        (CatalogProduct[])context.ParentContext.RootContextData[
                            ModuleConstants.ValidationContextData.ExistedProducts];

                    // do not check by outer id because id was set before validation if outer id exists
                    var productById = existedProducts.FirstOrDefault(p =>
                        record.Record.ProductId.EqualsInvariant(p.Id));

                    if (productById != null)
                    {
                        result = productById.CatalogId.EqualsInvariant(catalogId);
                    }

                    return result;
                })
                .When((record) => !string.IsNullOrEmpty(record.Record.ProductId))
                .WithMessage("The product does not belong to the catalog specified in the request.")
                .WithImportState();

            RuleFor(record => record.Record.ProductName)
                .Configure(rule => rule.CascadeMode = CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Product Name")
                .WithImportState()
                .MaximumLength(1024)
                .WithExceededMaxLengthCodeAndMessage("Product Name", 1024)
                .WithImportState();

            RuleFor(record => record.Record.ProductSku)
                .NotEmpty()
                .WithMissingRequiredValueCodeAndMessage("Product Sku")
                .WithImportState()
                .DependentRules(() =>
                {
                    RuleFor(record => record.Record.ProductSku)
                        .MaximumLength(64)
                        .WithExceededMaxLengthCodeAndMessage("Product Sku", 64)
                        .WithImportState()
                        .Must(sku => sku.IndexOfAny(ProductSkuIllegalCharacters) < 0)
                        .WithInvalidValueCodeAndMessage("Product Sku")
                        .WithImportState();
                });

            RuleFor(record => record.Record.CategoryId)
                .NotEmpty()
                .When(record => string.IsNullOrEmpty(record.Record.CategoryOuterId))
                .WithMissingRequiredValueCodeAndMessage("Category Id")
                .WithImportState();

            RuleFor(record => record.Record.CategoryOuterId)
                .NotEmpty()
                .When(record => string.IsNullOrEmpty(record.Record.CategoryId))
                .WithMissingRequiredValueCodeAndMessage("Category Outer Id")
                .WithImportState();

            RuleFor(record => record.Record.Gtin)
                .MaximumLength(64)
                .WithExceededMaxLengthCodeAndMessage("Gtin", 64)
                .WithImportState();

            RuleFor(record => record.Record.PackageType)
                .Configure(rule => rule.CascadeMode = CascadeMode.StopOnFirstFailure)
                .MaximumLength(254)
                .WithExceededMaxLengthCodeAndMessage("Package Type", 254)
                .WithImportState()
                .Must((record, packageType, context) =>
                {
                    var availablePackageTypes = (string[])context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.AvailablePackageTypes];
                    return availablePackageTypes.Contains(packageType);
                })
                .When(record => !string.IsNullOrEmpty(record.Record.PackageType))
                .WithInvalidValueCodeAndMessage("Package Type")
                .WithImportState();

            RuleFor(record => record.Record.ManufacturerPartNumber)
                .MaximumLength(128)
                .WithExceededMaxLengthCodeAndMessage("Manufacturer Part Number", 128)
                .WithImportState();

            RuleFor(record => record.Record.MeasureUnit)
                .Configure(rule => rule.CascadeMode = CascadeMode.StopOnFirstFailure)
                .MaximumLength(32)
                .WithExceededMaxLengthCodeAndMessage("Measure Unit", 32)
                .WithImportState()
                .Must((record, measureUnit, context) =>
                {
                    var availableMeasureUnits = (string[])context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.AvailableMeasureUnits];
                    return availableMeasureUnits.Contains(measureUnit);
                })
                .When(record => !string.IsNullOrEmpty(record.Record.MeasureUnit))
                .WithInvalidValueCodeAndMessage("Measure Unit")
                .WithImportState();

            RuleFor(record => record.Record.WeightUnit)
                .Configure(rule => rule.CascadeMode = CascadeMode.StopOnFirstFailure)
                .MaximumLength(32)
                .WithExceededMaxLengthCodeAndMessage("Weight Unit", 32)
                .WithImportState()
                .Must((record, weightUnit, context) =>
                {
                    var availableWeightUnits = (string[])context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.AvailableWeightUnits];
                    return availableWeightUnits.Contains(weightUnit);
                })
                .When(record => !string.IsNullOrEmpty(record.Record.WeightUnit))
                .WithInvalidValueCodeAndMessage("Weight Unit")
                .WithImportState();

            RuleFor(record => record.Record.TaxType)
                .Configure(rule => rule.CascadeMode = CascadeMode.StopOnFirstFailure)
                .MaximumLength(64)
                .WithExceededMaxLengthCodeAndMessage("Tax Type", 64)
                .WithImportState()
                .Must((record, taxType, context) =>
                {
                    var availableTaxTypes = (string[])context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.AvailableTaxTypes];
                    return availableTaxTypes.Contains(taxType);
                })
                .When(record => !string.IsNullOrEmpty(record.Record.TaxType))
                .WithInvalidValueCodeAndMessage("Tax Type")
                .WithImportState();

            RuleFor(record => record.Record.MainProductId)
                .Must((_, mainProductId, context) =>
                {
                    var existedMainProductIds =
                        (string[])context.ParentContext.RootContextData[
                            ModuleConstants.ValidationContextData.ExistedMainProductsIds];

                    var result = existedMainProductIds.Any(x => x.EqualsInvariant(mainProductId));

                    return result;
                })
                .When(record => !string.IsNullOrEmpty(record.Record.MainProductId))
                .WithNotExistedMainProduct()
                .WithImportState();

            // properties
            RuleFor(record => record.Record.Properties)
                .SetValidator(record => new ImportProductsPropertiesValidator(record));

        }
    }
}
