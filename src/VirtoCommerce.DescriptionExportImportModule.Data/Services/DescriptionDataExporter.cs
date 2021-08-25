using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.Extensions;

namespace VirtoCommerce.DescriptionExportImportModule.Data.Services
{
    public class DescriptionDataExporter : IDescriptionDataExporter
    {
        private readonly IDescriptionExportPagedDataSourceFactory _descriptionExportPagedDataSourceFactory;
        private readonly IExportWriterFactory _exportWriterFactory;
        private readonly PlatformOptions _platformOptions;
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IBlobUrlResolver _blobUrlResolver;

        public DescriptionDataExporter(
            IDescriptionExportPagedDataSourceFactory descriptionExportPagedDataSourceFactory,
            IExportWriterFactory exportWriterFactory,
            IOptions<PlatformOptions> platformOptions,
            IBlobStorageProvider blobStorageProvider,
            IBlobUrlResolver blobUrlResolver
            )
        {
            _descriptionExportPagedDataSourceFactory = descriptionExportPagedDataSourceFactory;
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
            var exportedDescriptionFilePath = GetExportFilePath("Descriptions");

            var dataSource = _descriptionExportPagedDataSourceFactory.Create(50, request); //TODO: Move page size to consts
            var exportWriter = _exportWriterFactory.Create(exportedDescriptionFilePath, new ExportConfiguration());

            exportProgress.TotalCount = await dataSource.GetTotalCountAsync();
            progressCallback(exportProgress);



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