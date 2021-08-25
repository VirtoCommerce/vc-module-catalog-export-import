using System;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface IExportWriter : IDisposable
    {
        void WriteRecords(CsvEditorialReview[] records);
    }
}
