using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface ICsvValidator
    {
        ValidationResult Validate<TImportable>(ValidationContext<ImportRecord<TImportable>[]> validationContext) where TImportable : IImportable;
    }
}
