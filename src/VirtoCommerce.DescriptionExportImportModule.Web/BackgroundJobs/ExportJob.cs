using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Hangfire;

namespace VirtoCommerce.DescriptionExportImportModule.Web.BackgroundJobs
{
    public sealed class ExportJob
    {
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly IDataExporter _dataExporter;

        public ExportJob(IPushNotificationManager pushNotificationManager, IDataExporter dataExporter)
        {
            _pushNotificationManager = pushNotificationManager;
            _dataExporter = dataExporter;
        }

        public async Task ExportBackgroundAsync(ExportDataRequest request, ExportPushNotification pushNotification, IJobCancellationToken jobCancellationToken, PerformContext context)
        {
            if (pushNotification is null)
            {
                throw new ArgumentException(nameof(pushNotification));
            }

            try
            {
                await _dataExporter.ExportAsync(request,
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
    }
}
