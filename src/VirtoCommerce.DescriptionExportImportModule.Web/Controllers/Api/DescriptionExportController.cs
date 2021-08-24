using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.DescriptionExportImportModule.Web.Controllers.Api
{
    [Route("api/catalog/description/export")]
    [Authorize]
    [ApiController]
    public class DescriptionExportController : ControllerBase
    {
        private readonly IUserNameResolver _userNameResolver;
        private readonly IPushNotificationManager _pushNotificationManager;

        public DescriptionExportController(IPushNotificationManager pushNotificationManager, IUserNameResolver userNameResolver)
        {
            _pushNotificationManager = pushNotificationManager;
            _userNameResolver = userNameResolver;
        }

        [HttpPost]
        [Route("run")]
        public async Task RunExport()
        {
            var notification = new ExportPushNotification(_userNameResolver.GetCurrentUserName());

            await _pushNotificationManager.SendAsync(notification);
        }

        [HttpPost]
        [Route("cancel")]
        public ActionResult CancelExport([FromBody] ExportCancellationRequest cancellationRequest)
        {
            BackgroundJob.Delete(cancellationRequest.JobId);
            return Ok();
        }
    }
}
