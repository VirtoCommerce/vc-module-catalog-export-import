using System;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface ICsvImportReporter : IAsyncDisposable
    {
        string FilePath { get; }
        bool ReportIsNotEmpty { get; }
        Task WriteHeaderAsync(string header);
        Task WriteAsync(ImportError error);
    }
}
