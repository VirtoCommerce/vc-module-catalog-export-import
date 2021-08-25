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

            var dataSource = _descriptionExportPagedDataSourceFactory.Create(50, request); //TODO: Move page size to consts

            exportProgress.TotalCount = await dataSource.GetTotalCountAsync();
            progressCallback(exportProgress);

            const string exportDescription = "{0} out of {1} have been exported.";

            exportProgress.Description = "Fetching...";
            progressCallback(exportProgress);

            try
            {
                while (await dataSource.FetchAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var descriptions = dataSource.Items;

                    exportProgress.ProcessedCount += dataSource.Items.Length;
                    exportProgress.Description = string.Format(exportDescription, exportProgress.ProcessedCount,
                        exportProgress.TotalCount);
                    progressCallback(exportProgress);
                }

                exportProgress.Description = "Export completed";
            }
            finally
            {

            }
        }
    }
}
