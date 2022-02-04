using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface ICsvValidatorFactory
    {
        ICsvValidator Create<TImportable>(
            IValidator<ImportRecord<TImportable>[]> importRecordsValidator, ICsvImportErrorReporter importErrorReporter)
            where TImportable : IImportable;
    }
}
