using System.IO;
using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public sealed class CsvImportReporter : ICsvImportReporter
    {
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly string _filePath;
        private readonly string _delimiter;
        private readonly StreamWriter _streamWriter;
        private const string ErrorsColumnName = "Error description";
        private readonly object _lock = new object();

        public bool ReportIsNotEmpty { get; private set; }

        public string FilePath => _filePath;

        public CsvImportReporter(string filePath, IBlobStorageProvider blobStorageProvider, string delimiter)
        {
            _filePath = filePath;
            _delimiter = delimiter;
            _blobStorageProvider = blobStorageProvider;
            var stream = _blobStorageProvider.OpenWrite(filePath);
            _streamWriter = new StreamWriter(stream);
        }

        public async Task WriteAsync(ImportError error)
        {
            using (await AsyncLock.GetLockByKey(_filePath).LockAsync())
            {
                ReportIsNotEmpty = true;
                await _streamWriter.WriteLineAsync(GetLine(error));
            }
        }

        public void Write(ImportError error)
        {
            lock (_lock)
            {
                ReportIsNotEmpty = true;
                _streamWriter.WriteLine(GetLine(error));
            }
        }

        public async Task WriteHeaderAsync(string header)
        {
            using (await AsyncLock.GetLockByKey(_filePath).LockAsync())
            {
                await _streamWriter.WriteLineAsync($"{ErrorsColumnName}{_delimiter}{header}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            using (await AsyncLock.GetLockByKey(_filePath).LockAsync())
            {
                await _streamWriter.FlushAsync();
                _streamWriter.Close();

                if (!ReportIsNotEmpty)
                {
                    await _blobStorageProvider.RemoveAsync(new[] { _filePath });
                }
            }
        }


        private string GetLine(ImportError importError)
        {
            var result = $"{importError.Error}{_delimiter}{importError.RawRow.TrimEnd()}";

            return result;
        }
    }
}
