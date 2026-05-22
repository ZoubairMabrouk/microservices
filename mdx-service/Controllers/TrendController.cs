using MdxServices.Interfaces;
using MdxServices.queries;
using MdxServices.Dto;
using Microsoft.AspNetCore.Mvc;

namespace MdxServices.Controllers
{
    [Route("api/trends")]
    public class TrendController : MdxControllerBase
    {
        public TrendController(IMdxService mdxService, ILogger<TrendController> logger)
            : base(mdxService, logger) { }

        [HttpGet("yoy-revenue/{currentYear:int}/{priorYear:int}")]
        public IActionResult YoyRevenue(int currentYear, int priorYear) =>
            RunQuery(DocumentCubeQueries.YoYRevenueByClient(currentYear, priorYear));

        [HttpGet("yoy-volume/{currentYear:int}/{priorYear:int}")]
        public IActionResult YoyVolume(int currentYear, int priorYear) =>
            RunQuery(DocumentCubeQueries.YoYVolumeByQuarter(currentYear, priorYear));

        [HttpGet("ytd/{year:int}")]
        public IActionResult Ytd(int year) =>
            RunQueries(DocumentCubeQueries.YtdRevenueByMonth(year), MdxMapper.ToYtdRevenue);

        [HttpGet("qtd/{year:int}")]
        public IActionResult Qtd(int year) =>
            RunQueries(DocumentCubeQueries.QtdRevenueByMonth(year), MdxMapper.ToQtdRevenue);

        [HttpGet("margin/{year:int}")]
        public IActionResult MarginTrend(int year) =>
            RunQueries(DocumentCubeQueries.MonthlyMarginTrend(year), MdxMapper.ToMonthlyMargin);
    }
}
