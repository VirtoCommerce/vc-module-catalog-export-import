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
        public ImportPhysicalProductSkuValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(record => record.Record.ProductSku)
                .Must((record, sku, context) =>
                {
                    var existedProductsWithSameSku =
                        (CatalogProduct[])context.ParentContext.RootContextData[
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
                .When(record => !string.IsNullOrEmpty(record.Record.ProductSku))
                .WithMessage("Product with the same SKU already exists in the current catalog in product with different Id.")
                .WithImportState();
        }
    }
}
