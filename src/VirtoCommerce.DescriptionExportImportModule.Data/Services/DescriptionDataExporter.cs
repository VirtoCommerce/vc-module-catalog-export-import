using System;
using System.Threading.Tasks;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class DescriptionDataExporter : IDescriptionDataExporter
    {
        private readonly IDescriptionExportPagedDataSourceFactory _descriptionExportPagedDataSourceFactory;

        public DescriptionDataExporter(IDescriptionExportPagedDataSourceFactory descriptionExportPagedDataSourceFactory)
        {
            _descriptionExportPagedDataSourceFactory = descriptionExportPagedDataSourceFactory;
        }

        public async Task ExportAsync(ExportDataRequest request, Action<ExportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new ExportProgressInfo { ProcessedCount = 0, Description = "Export has started" };

            var dataSource = _descriptionExportPagedDataSourceFactory.Create(50, request);

            exportProgress.TotalCount = await dataSource.GetTotalCountAsync();
            progressCallback(exportProgress);
        }
    }
}
