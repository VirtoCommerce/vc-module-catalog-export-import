using System;
using System.Threading.Tasks;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DescriptionExportImportModule.Core.Services
{
    public interface IDescriptionDataExporter
    {
        Task ExportAsync(ExportDataRequest request, Action<ExportProgressInfo> progressCallback, ICancellationToken cancellationToken);
    }
}
