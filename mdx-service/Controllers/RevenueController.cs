using MdxServices.Interfaces;
using MdxServices.queries;
using Microsoft.AspNetCore.Mvc;
using MdxServices.Dto;

namespace MdxServices.Controllers
{
    [Route("api/revenue")]
    public class RevenueController : MdxControllerBase
    {
        public RevenueController(IMdxService mdxService, ILogger<RevenueController> logger)
            : base(mdxService, logger) { }

        [HttpGet("by-year")]
        public IActionResult ByYear() =>
            RunQueries(DocumentCubeQueries.RevenueByYear, MdxMapper.ToYearRevenue);

        [HttpGet("by-month/{year:int}")]
        public IActionResult ByMonth(int year) =>
            RunQueries(DocumentCubeQueries.RevenueByMonth(year), MdxMapper.ToMonthRevenue);

        [HttpGet("by-quarter-doctype")]
        public IActionResult ByQuarterAndDocType() =>
            RunQueries(DocumentCubeQueries.RevenueByQuarterAndDocType, MdxMapper.ToQuarterRevenue);

        [HttpGet("by-client")]
        public IActionResult ByClient() =>
            RunQueries(DocumentCubeQueries.RevenueByClientDesc, MdxMapper.ToClientRevenue);

        [HttpGet("top-clients")]
        public IActionResult TopClients() =>
            RunQueries(DocumentCubeQueries.Top10ClientsByRevenue, MdxMapper.ToClientRevenue);

        [HttpGet("bottom-clients")]
        public IActionResult BottomClients() =>
            RunQueries(DocumentCubeQueries.Bottom10ClientsByRevenue, MdxMapper.ToClientRevenue);

        [HttpGet("concentration")]
        public IActionResult Concentration() =>
            RunQueries(DocumentCubeQueries.RevenueConcentrationTop20Pct, MdxMapper.ToRevenueConcentration);
    }
}
