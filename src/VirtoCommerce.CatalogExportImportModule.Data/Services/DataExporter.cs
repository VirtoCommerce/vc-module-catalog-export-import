using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogExportImportModule.Data.Helpers;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.Extensions;

namespace VirtoCommerce.CatalogExportImportModule.Data.Services
{
    public class DataExporter : IDataExporter
    {
        private readonly IExportPagedDataSourceFactory _exportPagedDataSourceFactory;
        private readonly IExportWriterFactory _exportWriterFactory;
        private readonly PlatformOptions _platformOptions;
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IBlobUrlResolver _blobUrlResolver;

        public DataExporter(
            IExportPagedDataSourceFactory exportPagedDataSourceFactory,
            IExportWriterFactory exportWriterFactory,
            IOptions<PlatformOptions> platformOptions,
            IBlobStorageProvider blobStorageProvider,
            IBlobUrlResolver blobUrlResolver
            )
        {
            _exportPagedDataSourceFactory = exportPagedDataSourceFactory;
            _exportWriterFactory = exportWriterFactory;
            _platformOptions = platformOptions.Value;
            _blobStorageProvider = blobStorageProvider;
            _blobUrlResolver = blobUrlResolver;
        }

        public async Task ExportAsync(ExportDataRequest request, Action<ExportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new ExportProgressInfo { ProcessedCount = 0, Description = "Export has started" };

            const string exportDescription = "{0} out of {1} have been exported.";
            var exportedDescriptionFilePath = GetExportFilePath(request.DataType.ToExportFileNamePrefix());

            var dataSource = _exportPagedDataSourceFactory.Create(ModuleConstants.Settings.PageSize, request);
            var exportWriter = _exportWriterFactory.Create(exportedDescriptionFilePath, new ExportConfiguration());

            exportProgress.TotalCount = await dataSource.GetTotalCountAsync();
            exportProgress.Description = "Fetching...";

            progressCallback(exportProgress);

            try
            {
                while (await dataSource.FetchAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var descriptions = dataSource.Items.OfType<CsvEditorialReview>().ToArray();

                    exportProgress.ProcessedCount += dataSource.Items.Length;
                    exportProgress.Description = string.Format(exportDescription, exportProgress.ProcessedCount,
                        exportProgress.TotalCount);
                    progressCallback(exportProgress);

                    exportWriter.WriteRecords(descriptions);
                }

                exportProgress.Description = "Export completed";
            }
            finally
            {
                exportWriter.Dispose();
            }

            try
            {
                var exportedDescriptionsFileInfo = await _blobStorageProvider.GetBlobInfoAsync(exportedDescriptionFilePath);

                if (exportedDescriptionsFileInfo.Size > 0)
                {
                    exportProgress.FileUrl = _blobUrlResolver.GetAbsoluteUrl(exportedDescriptionFilePath);
                }
                else
                {
                    await _blobStorageProvider.RemoveAsync(new[] { exportedDescriptionFilePath });
                }
            }
            finally
            {
                progressCallback(exportProgress);
            }
        }


        private string GetExportFilePath(string entityName)
        {
            if (string.IsNullOrEmpty(_platformOptions.DefaultExportFolder))
            {
                throw new PlatformException($"{nameof(_platformOptions.DefaultExportFolder)} must be set.");
            }

            const string template = "{0}_{1:yyyyMMddHHmmss}.csv";

            var result = string.Format(template, entityName, DateTime.UtcNow);

            return UrlHelperExtensions.Combine(_platformOptions.DefaultExportFolder, result);
        }
    }
}
