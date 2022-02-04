using System.Linq;
using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Data.Helpers;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Data.Validation
{
    public class ImportPhysicalProductSkuValidator : AbstractValidator<ImportRecord<CsvPhysicalProduct>>
    {
        private static readonly char[] ProductSkuIllegalCharacters = { '$', '+', ';', '=', '%', '{', '}', '[', ']', '|', '@', '~', '!', '^', '*', '&', '(', ')', '<', '>' };

        public ImportPhysicalProductSkuValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
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
                        .WithImportState()
                        .DependentRules(() =>
                        {
                            RuleFor(record => record.Record.ProductSku)
                                .Must((record, sku, context) =>
                                {
                                    var existedProductsWithSameSku =
                                        (CatalogProduct[])context.RootContextData[
                                            ModuleConstants.ValidationContextData.ExistedProductsWithSameSku];

                                    var productWithSuchSku = existedProductsWithSameSku.FirstOrDefault(x => x.Code.EqualsInvariant(sku));

                                    if (productWithSuchSku == null)
                                    {
                                        return true;
                                    }

                                    if ( // updating of product case.
                                         // do not check by outer id because id was set before validation if outer id exists
                                        (!string.IsNullOrEmpty(record.Record.ProductId) &&
                                         !productWithSuchSku.Id.EqualsInvariant(record.Record.ProductId))
                                        // creating of product case
                                        || (string.IsNullOrEmpty(record.Record.ProductId))
                                    )
                                    {
                                        return false;
                                    }

                                    return true;
                                })
                                .WithMessage(ModuleConstants.ValidationErrorMessages[ModuleConstants.ValidationErrorCodes.ProductWithSameSkuExists])
                                .WithImportState();
                        });
                });
        }
    }
}
