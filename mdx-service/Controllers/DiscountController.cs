using MdxServices.Interfaces;
using MdxServices.queries;
using Microsoft.AspNetCore.Mvc;
using MdxServices.Dto;

namespace MdxServices.Controllers
{
    [Route("api/discounts")]
    public class DiscountController : MdxControllerBase
    {
        public DiscountController(IMdxService mdxService, ILogger<DiscountController> logger)
            : base(mdxService, logger) { }

        [HttpGet("ratio-by-year")]
        public IActionResult RatioByYear() =>
            RunQueries(DocumentCubeQueries.DiscountRatioByYear, MdxMapper.ToDiscountRatioByYear);

        [HttpGet("top-clients")]
        public IActionResult TopClientsByDiscount() =>
            RunQueries(DocumentCubeQueries.Top10ClientsByDiscount, MdxMapper.ToClientDiscount);

        [HttpGet("by-document-type")]
        public IActionResult ByDocumentType() =>
            RunQueries(DocumentCubeQueries.DiscountByDocumentType, MdxMapper.ToDocumentDiscount);
        //RunQuery(DocumentCubeQueries.DiscountByDocumentType);
    }
}
