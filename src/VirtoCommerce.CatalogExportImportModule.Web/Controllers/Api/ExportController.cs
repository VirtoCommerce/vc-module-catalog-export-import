using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogExportImportModule.Data.Helpers;
using VirtoCommerce.CatalogExportImportModule.Web.BackgroundJobs;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CatalogExportImportModule.Web.Controllers.Api
{
    [Route("api/catalog/export")]
    [Authorize(ModuleConstants.Security.Permissions.ExportAccess)]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly IUserNameResolver _userNameResolver;
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly IProductEditorialReviewSearchService _productEditorialReviewSearchService;
        private readonly IExportDataRequestPreprocessor _requestPreprocessor;
        private readonly IExportProductSearchService _productSearchService;

        public ExportController(IPushNotificationManager pushNotificationManager, IUserNameResolver userNameResolver, IProductEditorialReviewSearchService productEditorialReviewSearchService, IExportDataRequestPreprocessor requestPreprocessor)
        {
            _pushNotificationManager = pushNotificationManager;
            _userNameResolver = userNameResolver;
            _productEditorialReviewSearchService = productEditorialReviewSearchService;
            _requestPreprocessor = requestPreprocessor;
        }

        [HttpPost]
        [Route("count")]
        public async Task<ActionResult<object>> GetTotalCount([FromBody] ExportDataRequest request)
        {
            var result = new ExportTotalCountResponse();

            await _requestPreprocessor.PreprocessRequestAsync(request);

            var criteria = request.ToExportProductSearchCriteria();
            criteria.Take = 0;

            switch (request.DataType)
            {
                case ModuleConstants.DataTypes.EditorialReview:
                    var reviewSearchResult = await _productEditorialReviewSearchService.SearchEditorialReviewsAsync(criteria);
                    result.TotalCount = reviewSearchResult.TotalCount;
                    break;
                case ModuleConstants.DataTypes.PhysicalProduct:
                    var productSearchResult = await _productSearchService.SearchAsync(criteria);
                    result.TotalCount = productSearchResult.TotalCount;
                    break;
            }

            return Ok();
        }

        [HttpPost]
        [Route("run")]
        public async Task<ActionResult<ExportPushNotification>> RunExport([FromBody] ExportDataRequest request)
        {
            var notification = new ExportPushNotification(_userNameResolver.GetCurrentUserName())
            {
                Title = ModuleConstants.PushNotificationsTitles.Export[ModuleConstants.DataTypes.EditorialReview],
                Description = "Starting export task...",
            };

            await _pushNotificationManager.SendAsync(notification);

            await _requestPreprocessor.PreprocessRequestAsync(request);

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
