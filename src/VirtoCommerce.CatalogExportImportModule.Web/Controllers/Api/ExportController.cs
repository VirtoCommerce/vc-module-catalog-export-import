using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogExportImportModule.Web.BackgroundJobs;
using VirtoCommerce.Platform.Core.Common;
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
        private readonly IExportPagedDataSourceFactory _exportPagedDataSourceFactory;

        public ExportController(IPushNotificationManager pushNotificationManager, IUserNameResolver userNameResolver, IProductEditorialReviewSearchService productEditorialReviewSearchService, IExportDataRequestPreprocessor requestPreprocessor, IExportPagedDataSourceFactory exportPagedDataSourceFactory)
        {
            _pushNotificationManager = pushNotificationManager;
            _userNameResolver = userNameResolver;
            _productEditorialReviewSearchService = productEditorialReviewSearchService;
            _requestPreprocessor = requestPreprocessor;
            _exportPagedDataSourceFactory = exportPagedDataSourceFactory;
        }

        [HttpPost]
        [Route("count")]
        public async Task<ActionResult<object>> GetTotalCount([FromBody] ExportDataRequest request)
        {
            var needExtendRequestWithChildCategories =
                request.DataType.EqualsInvariant(ModuleConstants.DataTypes.EditorialReview);

            await _requestPreprocessor.PreprocessRequestAsync(request, needExtendRequestWithChildCategories);

            var dataSource = _exportPagedDataSourceFactory.Create(0, request);

            var totalCount = await dataSource.GetTotalCountAsync();



            //var criteria = request.ToExportProductSearchCriteria();
            //criteria.Take = 0;

            //switch (request.DataType)
            //{
            //    case ModuleConstants.DataTypes.EditorialReview:
            //        var reviewSearchResult = await _productEditorialReviewSearchService.SearchEditorialReviewsAsync(criteria);
            //        result.TotalCount = reviewSearchResult.TotalCount;
            //        break;
            //    case ModuleConstants.DataTypes.PhysicalProduct:
            //        var productSearchResult = await _productSearchService.SearchAsync(criteria);
            //        result.TotalCount = productSearchResult.TotalCount;
            //        break;
            //}

            return Ok(new ExportTotalCountResponse { TotalCount = totalCount });
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
