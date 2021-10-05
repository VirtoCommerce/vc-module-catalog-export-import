using System;
using System.Threading.Tasks;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogExportImportModule.Core.Services
{
    public interface ICsvPagedDataImporter
    {
        string DataType { get; }

        Task ImportAsync(ImportDataRequest request, Action<ImportProgressInfo> progressCallback, ICancellationToken cancellationToken);
    }
}
