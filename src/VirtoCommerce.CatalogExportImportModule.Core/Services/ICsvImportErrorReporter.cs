using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface ICsvImportErrorReporter : IAsyncDisposable
    {
        bool ReportIsNotEmpty { get; }

        void WriteHeader(string header);

        void Write(int row, string rawRow);

        void Write(ImportParsingError error);

        void Write(ImportValidationError error);

        Task FlushAsync();
    }
}
