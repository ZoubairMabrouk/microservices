using MdxServices.Interfaces;
using MdxServices.queries;
using Microsoft.AspNetCore.Mvc;
using MdxServices.Dto;
namespace MdxServices.Controllers
{
    [Route("api/tax")]
    public class TaxController : MdxControllerBase
    {
        public TaxController(IMdxService mdxService, ILogger<TaxController> logger)
            : base(mdxService, logger) { }

        [HttpGet("tva-by-month/{year:int}")]
        public IActionResult TvaByMonth(int year) =>
            RunQueries(DocumentCubeQueries.TvaByMonth(year), MdxMapper.ToTvaMonth);

        [HttpGet("fodec-by-doctype")]
        public IActionResult FodecByDocType() =>
            RunQueries(DocumentCubeQueries.FodecByDocumentType, MdxMapper.ToFodec);

        [HttpGet("burden-by-quarter")]
        public IActionResult BurdenByQuarter() =>
            RunQueries(DocumentCubeQueries.TaxBurdenRatioByQuarter, MdxMapper.ToTaxQuarter);
    }
}
