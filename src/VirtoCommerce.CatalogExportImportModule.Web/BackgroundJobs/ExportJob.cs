using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Hangfire;

namespace VirtoCommerce.CatalogExportImportModule.Web.BackgroundJobs
{
    public sealed class ExportJob
    {
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly IEnumerable<IDataExporter> _dataExporters;

        public ExportJob(IPushNotificationManager pushNotificationManager, IEnumerable<IDataExporter> dataExporters)
        {
            _pushNotificationManager = pushNotificationManager;
            _dataExporters = dataExporters;
        }

        public async Task ExportBackgroundAsync(ExportDataRequest request, ExportPushNotification pushNotification, IJobCancellationToken jobCancellationToken, PerformContext context)
        {
            ValidateParameters(request, pushNotification);

            try
            {
                var dataExporter = _dataExporters.First(x => x.DataType == request.DataType);

                await dataExporter.ExportAsync(request,
                    progressInfo => ProgressCallback(progressInfo, pushNotification, context),
                    new JobCancellationTokenWrapper(jobCancellationToken));
            }
            catch (JobAbortedException)
            {
                // job is aborted, nothing to do
            }
            catch (Exception ex)
            {
                pushNotification.Errors?.Add(ex.ExpandExceptionMessage());
            }
            finally
            {
                pushNotification.Description = "Export finished";
                pushNotification.Finished = DateTime.UtcNow;
                await _pushNotificationManager.SendAsync(pushNotification);
            }
        }

        private void ProgressCallback(ExportProgressInfo x, ExportPushNotification pushNotification, PerformContext context)
        {
            pushNotification.Description = x.Description;
            pushNotification.ProcessedCount = x.ProcessedCount;
            pushNotification.TotalCount = x.TotalCount;
            pushNotification.JobId = context.BackgroundJob.Id;
            pushNotification.FileUrl = x.FileUrl;

            _pushNotificationManager.Send(pushNotification);
        }

        private void ValidateParameters(ExportDataRequest request, ExportPushNotification pushNotification)
        {
            if (pushNotification == null)
            {
                throw new ArgumentNullException(nameof(pushNotification));
            }

            var importer = _dataExporters.FirstOrDefault(x => x.DataType == request.DataType);

            if (importer == null)
            {
                throw new ArgumentException($"Not allowed argument value in field {nameof(request.DataType)}", nameof(request));
            }
        }
    }
}
