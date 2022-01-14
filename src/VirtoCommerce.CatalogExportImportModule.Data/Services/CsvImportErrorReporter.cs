using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public sealed class CsvImportErrorReporter : ICsvImportErrorReporter
    {
        private const string ErrorsColumnName = "Error description";
        private const string MessageDelimiter = "\\; ";

        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly string _filePath;
        private readonly string _delimiter;
        private readonly StreamWriter _streamWriter;

        private readonly ConcurrentDictionary<int, string> _rawRows = new ConcurrentDictionary<int, string>();
        private readonly ConcurrentBag<ImportParsingError> _parsingErrors = new ConcurrentBag<ImportParsingError>();
        private readonly ConcurrentBag<ImportValidationError> _validationErrors = new ConcurrentBag<ImportValidationError>();

        public bool ReportIsNotEmpty { get; private set; }

        public CsvImportErrorReporter(IBlobStorageProvider blobStorageProvider, string filePath, string delimiter)
        {
            _filePath = filePath;
            _delimiter = delimiter;
            _blobStorageProvider = blobStorageProvider;
            var stream = _blobStorageProvider.OpenWrite(filePath);
            _streamWriter = new StreamWriter(stream);
        }

        public void WriteHeader(string header)
        {
            _streamWriter.WriteLine($"{ErrorsColumnName}{_delimiter}{header}");
        }

        public void Write(int row, string rawRow)
        {
            _rawRows.AddOrUpdate(row, rawRow, (_, __) => rawRow);
        }

        public void Write(ImportParsingError error)
        {
            _parsingErrors.Add(error);
        }

        public void Write(ImportValidationError error)
        {
            _validationErrors.Add(error);
        }

        public async Task FlushAsync()
        {
            ReportIsNotEmpty = true;

            // Group by row number
            // Group by error code (=same errors with different arguments)
            // Concatenate parsing error messages
            var parsingErrors = _parsingErrors
                .GroupBy(parsingError => parsingError.Row)
                .GroupBy(parsingErrors => parsingErrors.Key, parsingErrors => parsingErrors.GroupBy(parsingError => parsingError.ErrorCode))
                .Select(parsingErrorsPerRow =>
                {
                    var row = parsingErrorsPerRow.Key;
                    var errorMessages = parsingErrorsPerRow.Select(parsingErrorsPerCode =>
                    {
                        var errorMessagesPerCode = parsingErrorsPerCode.Select(sameCodeParsingErrors =>
                        {
                            var errorCode = sameCodeParsingErrors.Key;
                            var parameters = sameCodeParsingErrors.SelectMany(parsingError => parsingError.Parameters).Cast<object>().ToArray();
                            return ModuleConstants.GetParsingErrorMessage(errorCode, parameters);
                        });
                        return ConcatErrorMessages(errorMessagesPerCode);
                    });
                    return new { Message = ConcatErrorMessages(errorMessages), Row = row };
                });
            
            // Group by row number
            // Concatenate validation error messages
            var validationErrors = _validationErrors
                .GroupBy(validationError => validationError.Row)
                .Select(validationErrorsPerRow =>
                {
                    var row = validationErrorsPerRow.Key;
                    var errorMessages = validationErrorsPerRow.Select(validationError => validationError.ErrorMessage);
                    return new { Message = ConcatErrorMessages(errorMessages), Row = row };
                });

            // Concatenate parsing & validation error messages,
            // order them by row...
            var errors = parsingErrors.Concat(validationErrors)
                .GroupBy(error => error.Row)
                .Select(errorsPerRow => new { Message = ConcatErrorMessages(errorsPerRow.Select(error => error.Message)), Row = errorsPerRow.Key })
                .OrderBy(errorsPerRow => errorsPerRow.Row);

            //... and then write
            foreach (var error in errors)
            {
                var rawRow = _rawRows[error.Row];
                await _streamWriter.WriteLineAsync($"{error.Message}{_delimiter}{rawRow.TrimEnd()}");
            }

            // Cleanup stored rows and errors
            _rawRows.Clear();
            _parsingErrors.Clear();
            _validationErrors.Clear();
        }

        public async ValueTask DisposeAsync()
        {
            await _streamWriter.FlushAsync();
            _streamWriter.Close();

            if (!ReportIsNotEmpty)
            {
                await _blobStorageProvider.RemoveAsync(new[] { _filePath });
            }
        }

        private string ConcatErrorMessages(IEnumerable<string> errorMessages)
        {
            return string.Join(MessageDelimiter, errorMessages);
        }
    }
}
