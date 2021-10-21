using System;
using CsvHelper.Configuration;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IExportWriter<TExportable> : IDisposable where TExportable : IExportable
    {
        void WriteRecords(TExportable[] records);

        void RegisterClassMap(ClassMap<TExportable> classMap);
    }
}
