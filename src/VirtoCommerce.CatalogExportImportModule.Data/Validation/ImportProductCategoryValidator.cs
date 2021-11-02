using System.Linq;
using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Data.Helpers;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Data.Validation
{
    public class ImportProductCategoryValidator : AbstractValidator<ImportRecord<CsvPhysicalProduct>>
    {
        public ImportProductCategoryValidator()
        {
            AttachValidators();
        }

        private void AttachValidators()
        {
            RuleFor(record => record)
                .Configure(rule => rule.CascadeMode = CascadeMode.StopOnFirstFailure)
                .Must((record, _, context) =>
                {
                    var existedCategories =
                        (Category[])context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.ExistedCategories];

                    // do not check by outer category id because category id was set before validation if outer id exists
                    var result = existedCategories.Any(c =>
                        record.Record.CategoryId.EqualsInvariant(c.Id));

                    return result;
                })
                .When(record => !string.IsNullOrEmpty(record.Record.CategoryId) || !string.IsNullOrEmpty(record.Record.CategoryOuterId))
                .WithMessage("Such Category does not exist in the system.")
                .WithImportState()
                .Must((record, _, context) =>
                {
                    var catalogId =
                        context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.CatalogId] as string;

                    var existedCategories =
                        (Category[])context.ParentContext.RootContextData[ModuleConstants.ValidationContextData.ExistedCategories];

                    var category = existedCategories.First(c => c.Id.EqualsInvariant(record.Record.CategoryId));

                    var result = catalogId.EqualsInvariant(category.CatalogId);

                    return result;
                })
                .WithMessage("Category does not belong to the catalog of the import request.")
                .WithImportState();
        }
    }
}
