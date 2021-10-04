using System;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Assets;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class ExportWriter : IExportWriter
    {
        private readonly StreamWriter _streamWriter;
        private readonly CsvWriter _csvWriter;

        public ExportWriter(string filePath, IBlobStorageProvider blobStorageProvider, CsvConfiguration csvConfiguration)
        {
            var stream = blobStorageProvider.OpenWrite(filePath);
            _streamWriter = new StreamWriter(stream);
            _csvWriter = new CsvWriter(_streamWriter, csvConfiguration);
        }

        public void WriteRecords(CsvEditorialReview[] records)
        {
            _csvWriter.WriteRecords(records);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _streamWriter.Flush();
            _streamWriter.Close();
            _csvWriter.Dispose();
        }
    }
}
