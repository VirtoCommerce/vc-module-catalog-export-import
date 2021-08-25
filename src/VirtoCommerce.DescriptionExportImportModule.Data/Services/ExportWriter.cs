using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class ExportWriter
    {
        private readonly StreamWriter _streamWriter;
        private readonly CsvWriter _csvWriter;

        public ExportWriter(string filePath, IBlobStorageProvider blobStorageProvider, Configuration csvConfiguration)
        {
            var stream = blobStorageProvider.OpenWrite(filePath);
            _streamWriter = new StreamWriter(stream);
            _csvWriter = new CsvWriter(_streamWriter, csvConfiguration);
        }

        public void WriteRecords(EditorialReview[] records)
        {
            _csvWriter.WriteRecords(records);
        }

        public void Dispose()
        {
            _streamWriter.Flush();
            _streamWriter.Close();
            _csvWriter.Dispose();
        }
    }
}
