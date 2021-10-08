using System;
using VirtoCommerce.CatalogExportImportModule.Core.Models;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IExportWriter<in TExportable> : IDisposable where TExportable : IExportable
    {
        void WriteRecords(TExportable[] records);
    }
}
