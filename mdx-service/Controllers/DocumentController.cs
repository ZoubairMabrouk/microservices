using MdxServices.Interfaces;
using MdxServices.queries;
using Microsoft.AspNetCore.Mvc;
using MdxServices.Dto;
namespace MdxServices.Controllers
{
    [Route("api/documents")]
    public class DocumentController : MdxControllerBase
    {
        public DocumentController(IMdxService mdxService, ILogger<DocumentController> logger)
            : base(mdxService, logger) { }

        /// <summary>Volume sold by document type × quarter crossjoin.</summary>
        [HttpGet("volume-by-type-quarter")]
        public IActionResult VolumeByTypeAndQuarter() =>
            RunQueries(DocumentCubeQueries.VolumeByDocTypeAndQuarter, MdxMapper.ToVolumeByDocTypeAndQuarter);
    }
}
