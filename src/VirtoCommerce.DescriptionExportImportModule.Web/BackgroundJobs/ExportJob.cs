using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.Exceptions;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Hangfire;

namespace VirtoCommerce.DescriptionExportImportModule.Web.BackgroundJobs
{
    public sealed class ExportJob
    {
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly IDescriptionDataExporter _descriptionDataExporter;

        public ExportJob(IPushNotificationManager pushNotificationManager, IDescriptionDataExporter descriptionDataExporter)
        {
            _pushNotificationManager = pushNotificationManager;
            _descriptionDataExporter = descriptionDataExporter;
        }

        public async Task ExportBackgroundAsync(ExportDataRequest request, ExportPushNotification pushNotification, IJobCancellationToken jobCancellationToken, PerformContext context)
        {
            if (pushNotification is null)
            {
                throw new ArgumentException(nameof(pushNotification));
            }

            try
            {
                await _descriptionDataExporter.ExportAsync(request,
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

            _pushNotificationManager.Send(pushNotification);
        }
    }
}