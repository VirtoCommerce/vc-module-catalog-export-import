using FluentValidation;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public sealed class CsvValidatorFactory: ICsvValidatorFactory
    {
        public ICsvValidator Create<TImportable>(IValidator<ImportRecord<TImportable>[]> importRecordsValidator, ICsvImportErrorReporter importErrorReporter)
            where TImportable : IImportable
        {
            return new CsvValidator<TImportable>(importRecordsValidator, importErrorReporter);
        }
    }
}
