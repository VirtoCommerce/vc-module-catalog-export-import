using System;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IExportWriter : IDisposable
    {
        void WriteRecords(CsvEditorialReview[] records);
    }
}
