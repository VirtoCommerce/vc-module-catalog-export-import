using System;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.AssetsModule.Core.Assets;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class ExportWriter<TExportable> : IExportWriter<TExportable> where TExportable : IExportable
    {
        private readonly StreamWriter _streamWriter;
        private readonly CsvWriter _csvWriter;

        public ExportWriter(string filePath, IBlobStorageProvider blobStorageProvider, CsvConfiguration csvConfiguration)
        {
            var stream = blobStorageProvider.OpenWrite(filePath);
            _streamWriter = new StreamWriter(stream);
            _csvWriter = new CsvWriter(_streamWriter, csvConfiguration);
        }

        public void RegisterClassMap(ClassMap<TExportable> classMap)
        {
            _csvWriter.Context.RegisterClassMap(classMap);
        }

        public void WriteRecords(TExportable[] records)
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
            _csvWriter.Dispose();
            _streamWriter.Dispose();
        }
    }
}
