using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public sealed class CsvValidator<T> : ICsvValidator
        where T: IImportable
    {
        private readonly IValidator<ImportRecord<T>[]> _importRecordsValidator;
        private readonly ICsvImportErrorReporter _importErrorReporter;

        public CsvValidator(IValidator<ImportRecord<T>[]> importRecordsValidator, ICsvImportErrorReporter importErrorReporter)
        {
            _importRecordsValidator = importRecordsValidator;
            _importErrorReporter = importErrorReporter;
        }

        public ValidationResult Validate<TImportable>(ValidationContext<ImportRecord<TImportable>[]> validationContext)
            where TImportable: IImportable
        {
            var validationResult = _importRecordsValidator.Validate(validationContext);

            var validationFailures = validationResult.Errors
                .Select(x => new { Message = x.ErrorMessage, (x.CustomState as ImportValidationState<TImportable>)?.InvalidRecord })
                .ToArray();

            // We need to order by row number because otherwise records will be written to report in random order
            var validationFailuresPerRow = validationFailures.GroupBy(x => x.InvalidRecord).OrderBy(x => x.Key.Row);

            foreach (var sameRowValidationFailures in validationFailuresPerRow)
            {
                var record = sameRowValidationFailures.Key;

                var errorMessages = string.Join(" ", sameRowValidationFailures.Select(x => x.Message).ToArray());

                var validationError = new ImportValidationError
                {
                    ErrorMessage = errorMessages,
                    Row = record.Row
                };

                _importErrorReporter.Write(record.Row, record.RawRecord);
                _importErrorReporter.Write(validationError);
            }

            return validationResult;
        }
    }
}
