using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface ICsvImportReporter : IAsyncDisposable
    {
        string FilePath { get; }
        bool ReportIsNotEmpty { get; }
        Task WriteHeaderAsync(string header);
        Task WriteAsync(ImportError error);
        void Write(ImportError error);
    }
}
