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
    public abstract class DataExporter<TExportable> : IDataExporter where TExportable : IExportable
    {
        private readonly IExportPagedDataSourceFactory _exportPagedDataSourceFactory;
        private readonly IExportWriterFactory _exportWriterFactory;
        private readonly PlatformOptions _platformOptions;
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IBlobUrlResolver _blobUrlResolver;

        protected DataExporter(
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
        public abstract string DataType { get; }

        public async Task ExportAsync(ExportDataRequest request, Action<ExportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var exportProgress = new ExportProgressInfo { ProcessedCount = 0, Description = "Export has started" };

            const string exportDescription = "{0} out of {1} have been exported.";
            var exportFilePath = GetExportFilePath(request.DataType.ToExportFileNamePrefix());

            var dataSource = _exportPagedDataSourceFactory.Create(ModuleConstants.Settings.PageSize, request);
            var exportWriter = _exportWriterFactory.Create<TExportable>(exportFilePath, new ExportConfiguration());

            exportProgress.TotalCount = await dataSource.GetTotalCountAsync();
            exportProgress.Description = "Fetching...";

            progressCallback(exportProgress);

            try
            {
                while (await dataSource.FetchAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var exportItems = dataSource.Items.OfType<TExportable>().ToArray();

                    exportProgress.ProcessedCount += exportItems.Length;
                    exportProgress.Description = string.Format(exportDescription, exportProgress.ProcessedCount,
                        exportProgress.TotalCount);
                    progressCallback(exportProgress);

                    exportWriter.WriteRecords(exportItems);
                }

                exportProgress.Description = "Export completed";
            }
            finally
            {
                exportWriter.Dispose();
            }

            try
            {
                var exportedDescriptionsFileInfo = await _blobStorageProvider.GetBlobInfoAsync(exportFilePath);

                if (exportedDescriptionsFileInfo.Size > 0)
                {
                    exportProgress.FileUrl = _blobUrlResolver.GetAbsoluteUrl(exportFilePath);
                }
                else
                {
                    await _blobStorageProvider.RemoveAsync(new[] { exportFilePath });
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
