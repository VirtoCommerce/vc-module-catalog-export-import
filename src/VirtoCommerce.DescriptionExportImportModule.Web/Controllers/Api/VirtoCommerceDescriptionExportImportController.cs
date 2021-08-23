using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.DescriptionExportImportModule.Core;


namespace VirtoCommerce.DescriptionExportImportModule.Web.Controllers.Api
{
    [Route("api/VirtoCommerceDescriptionExportImport")]
    public class VirtoCommerceDescriptionExportImportController : Controller
    {
        // GET: api/VirtoCommerceDescriptionExportImport
        /// <summary>
        /// Get message
        /// </summary>
        /// <remarks>Return "Hello world!" message</remarks>
        [HttpGet]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public ActionResult<string> Get()
        {
            return Ok(new { result = "Hello world!" });
        }
    }
}
