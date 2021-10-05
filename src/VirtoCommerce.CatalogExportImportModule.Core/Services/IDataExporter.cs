using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface IDataExporter
    {
        Task ExportAsync(ExportDataRequest request, Action<ExportProgressInfo> progressCallback, ICancellationToken cancellationToken);
    }
}
