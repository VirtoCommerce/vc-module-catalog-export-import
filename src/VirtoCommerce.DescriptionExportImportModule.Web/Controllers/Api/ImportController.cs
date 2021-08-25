using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.DescriptionExportImportModule.Core;
using VirtoCommerce.DescriptionExportImportModule.Core.Models;
using VirtoCommerce.DescriptionExportImportModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DescriptionExportImportModule.Web.Controllers.Api
{
    [Route("api/description/import")]
    [Authorize(ModuleConstants.Security.Permissions.ImportAccess)]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly ICsvDataValidator _csvDataValidator;

        public ImportController(ICsvDataValidator csvDataValidator)
        {
            _csvDataValidator = csvDataValidator;
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
    }
}
