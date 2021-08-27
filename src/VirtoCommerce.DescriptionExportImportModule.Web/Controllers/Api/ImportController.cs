using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.DescriptionExportImportModule.Core;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.DescriptionExportImportModule.Web.BackgroundJobs;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.DescriptionExportImportModule.Web.Controllers.Api
{
    [Route("api/description/import")]
    [Authorize(ModuleConstants.Security.Permissions.ImportAccess)]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly ICsvDataValidator _csvDataValidator;
        private readonly IUserNameResolver _userNameResolver;
        private readonly IPushNotificationManager _pushNotificationManager;

        public ImportController(ICsvDataValidator csvDataValidator,
            IUserNameResolver userNameResolver, IPushNotificationManager pushNotificationManager)
        {
            _csvDataValidator = csvDataValidator;
            _userNameResolver = userNameResolver;
            _pushNotificationManager = pushNotificationManager;
        }

        [HttpPost]
        [Route("validate")]
        public async Task<ActionResult<ImportDataValidationResult>> Validate([FromBody] ImportDataValidationRequest request)
        {
            if (request.FilePath.IsNullOrEmpty())
            {
                return BadRequest($"{nameof(request.FilePath)} can not be null or empty.");
            }

            var result = await _csvDataValidator.ValidateAsync(request.DataType, request.FilePath);

            return Ok(result);
        }


        [HttpPost]
        [Route("run")]
        public async Task<ActionResult<ExportPushNotification>> RunImport([FromBody] ImportDataRequest request)
        {
            var notification = new ImportPushNotification(_userNameResolver.GetCurrentUserName())
            {
                Title = "Descriptions import",
                Description = "Starting import task..."
            };

            await _pushNotificationManager.SendAsync(notification);

            notification.JobId = BackgroundJob.Enqueue<ImportJob>(importJob => importJob.ImportBackgroundAsync(request, notification, JobCancellationToken.Null, null));

            return Ok(notification);
        }

        [HttpPost]
        [Route("cancel")]
        public ActionResult CancelImport([FromBody] ImportCancellationRequest cancellationRequest)
        {
            BackgroundJob.Delete(cancellationRequest.JobId);
            return Ok();
        }
    }
}
