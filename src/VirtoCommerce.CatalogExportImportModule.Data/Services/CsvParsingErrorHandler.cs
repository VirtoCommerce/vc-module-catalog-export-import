using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public sealed class CsvParsingErrorHandler: ICsvParsingErrorHandler
    {
        private readonly ICsvImportErrorReporter _importErrorReporter;

        public CsvParsingErrorHandler(ICsvImportErrorReporter importErrorReporter)
        {
            _importErrorReporter = importErrorReporter;
        }

        public void HandleErrors(CsvConfiguration configuration)
        {
            configuration.ReadingExceptionOccurred = args =>
            {
                var context = args.Exception.Context;
                var rawValue = context.Reader[context.Reader.CurrentIndex];

                if (rawValue == string.Empty)
                {
                    // Value is missed; if columns is not required, then MissedFieldFound will be called
                    HandleMissedValueError(context, true);
                }
                else
                {
                    // Value can't be parsed
                    HandleWrongValueError(context);
                }

                return false;
            };

            // Not escaped quotes only
            configuration.BadDataFound = args => HandleBadDataError(args.Context);

            // Value is missed; if column is required, then ReadingExceptionOccurred will be called and raw value will be empty (missed)
            configuration.MissingFieldFound = args => HandleMissedValueError(args.Context, false);
        }

        private void HandleBadDataError(CsvContext context)
        {
            var parsingError = new ImportParsingError
            {
                ErrorCode = ModuleConstants.ParsingErrorCodes.NotEscapedQuote,
                Row = context.Parser.Row
            };

            _importErrorReporter.Write(context.Parser.Row, context.Parser.RawRecord);
            _importErrorReporter.Write(parsingError);

            throw new BadDataException(context, "Exception to prevent double BadDataFound call");
        }

        private void HandleWrongValueError(CsvContext context)
        {
            var columnName = context.Reader.HeaderRecord[context.Reader.CurrentIndex];
            var parsingError = new ImportParsingError
            {
                ErrorCode = ModuleConstants.ParsingErrorCodes.InvalidValue,
                Parameters = new [] { columnName },
                Row = context.Parser.Row
            };

            _importErrorReporter.Write(context.Parser.Row, context.Parser.RawRecord);
            _importErrorReporter.Write(parsingError);
        }

        private void HandleMissedValueError(CsvContext context, bool isRequired)
        {
            var columnName = context.Reader.HeaderRecord[context.Reader.CurrentIndex];
            var parsingError = new ImportParsingError
            {
                ErrorCode = isRequired ? ModuleConstants.ParsingErrorCodes.MissedRequiredValue : ModuleConstants.ParsingErrorCodes.MissedValue,
                Parameters = new[] { columnName },
                Row = context.Parser.Row
            };

            _importErrorReporter.Write(context.Parser.Row, context.Parser.RawRecord);
            _importErrorReporter.Write(parsingError);
        }
    }
}
