using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogExportImportModule.Core;
using VirtoCommerce.CatalogExportImportModule.Core.Models;
using VirtoCommerce.CatalogExportImportModule.Core.Services;
using VirtoCommerce.CatalogExportImportModule.Data.Helpers;
using VirtoCommerce.CatalogExportImportModule.Web.BackgroundJobs;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.PushNotifications;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CatalogExportImportModule.Web.Controllers.Api
{
    [Route("api/catalog/import")]
    [Authorize(ModuleConstants.Security.Permissions.ImportAccess)]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly ICsvDataValidator _csvDataValidator;
        private readonly IUserNameResolver _userNameResolver;
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly IBlobStorageProvider _blobStorageProvider;
        private readonly IImportPagedDataSourceFactory _importPagedDataSourceFactory;
        private readonly IImportProductsClassMapFactory _importProductsClassMapFactory;

        public ImportController(ICsvDataValidator csvDataValidator,
            IUserNameResolver userNameResolver, IPushNotificationManager pushNotificationManager,
            IBlobStorageProvider blobStorageProvider,
            IImportPagedDataSourceFactory importPagedDataSourceFactory,
            IImportProductsClassMapFactory importProductsClassMapFactory)
        {
            _csvDataValidator = csvDataValidator;
            _userNameResolver = userNameResolver;
            _pushNotificationManager = pushNotificationManager;
            _blobStorageProvider = blobStorageProvider;
            _importPagedDataSourceFactory = importPagedDataSourceFactory;
            _importProductsClassMapFactory = importProductsClassMapFactory;

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
                Title = request.DataType.ToImportPushNotificationTitle(),
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

        [HttpPost]
        [Route("preview")]
        public async Task<ActionResult<ImportDataPreview>> GetImportPreview([FromBody] ImportDataPreviewRequest request)
        {
            if (request.CatalogId.IsNullOrEmpty())
            {
                return BadRequest($"{nameof(request.CatalogId)} can not be null");
            }

            if (request.FilePath.IsNullOrEmpty())
            {
                return BadRequest($"{nameof(request.FilePath)} can not be null");
            }

            var blobInfo = await _blobStorageProvider.GetBlobInfoAsync(request.FilePath);

            if (blobInfo == null)
            {
                return BadRequest("Blob with the such url does not exist.");
            }

            var result = new ImportDataPreview();

            switch (request.DataType)
            {
                case ModuleConstants.DataTypes.EditorialReview:
                    using (var csvDataSource = _importPagedDataSourceFactory.Create<CsvEditorialReview>(request.FilePath,
                        10, null))
                    {
                        result.TotalCount = csvDataSource.GetTotalCount();
                        await csvDataSource.FetchAsync();
                        result.Results = csvDataSource.Items.Select(item => item.Record).ToArray();
                    }
                    break;

                case ModuleConstants.DataTypes.PhysicalProduct:
                    using (var csvDataSource = _importPagedDataSourceFactory.Create<CsvPhysicalProduct>(request.FilePath,
                        10, null))
                    {
                        var importProductsClassMap =
                            await _importProductsClassMapFactory.CreateClassMapAsync(request.CatalogId);

                        csvDataSource.RegisterClassMap(importProductsClassMap);

                        result.TotalCount = csvDataSource.GetTotalCount();
                        await csvDataSource.FetchAsync();
                        result.Results = csvDataSource.Items.Select(item => item.Record).ToArray();
                    }
                    break;

            }

            return Ok(result);
        }
    }
}
