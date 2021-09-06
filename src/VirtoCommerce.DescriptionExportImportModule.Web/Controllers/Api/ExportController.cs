using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.DescriptionExportImportModule.Web.BackgroundJobs;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.DescriptionExportImportModule.Web.Controllers.Api
{
    [Route("api/description/export")]
    [Authorize]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly IUserNameResolver _userNameResolver;
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly IProductEditorialReviewSearchService _productEditorialReviewSearchService;

        public ExportController(IPushNotificationManager pushNotificationManager, IUserNameResolver userNameResolver, IProductEditorialReviewSearchService productEditorialReviewSearchService)
        {
            _pushNotificationManager = pushNotificationManager;
            _userNameResolver = userNameResolver;
            _productEditorialReviewSearchService = productEditorialReviewSearchService;
        }

        [HttpPost]
        [Route("count")]
        public async Task<ActionResult<object>> GetTotalCount([FromBody] ExportDataRequest request)
        {
            var criteria = request.ToSearchCriteria();
            criteria.Take = 0;

            var searchResult = await _productEditorialReviewSearchService.SearchEditorialReviewsAsync(criteria, deepSearch: true);

            return Ok(new { searchResult.TotalCount });
        }

        [HttpPost]
        [Route("run")]
        public async Task<ActionResult<ExportPushNotification>> RunExport([FromBody] ExportDataRequest request)
        {
            var notification = new ExportPushNotification(_userNameResolver.GetCurrentUserName())
            {
                Title = "Product descriptions export",
                Description = "Starting export task...",
            };

            await _pushNotificationManager.SendAsync(notification);

            notification.JobId = BackgroundJob.Enqueue<ExportJob>(exportJob => exportJob.ExportBackgroundAsync(request, notification, JobCancellationToken.Null, null));

            return Ok(notification);
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
