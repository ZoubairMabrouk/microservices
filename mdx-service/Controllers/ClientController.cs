using MdxServices.Interfaces;
using MdxServices.queries;
using Microsoft.AspNetCore.Mvc;
using MdxServices.Dto;

namespace MdxServices.Controllers
{
    [Route("api/clients")]
    public class ClientController : MdxControllerBase
    {
        public ClientController(IMdxService mdxService, ILogger<ClientController> logger)
            : base(mdxService, logger) { }

        /// <summary>Clients with revenue above the portfolio average.</summary>
        [HttpGet("above-average")]
        public IActionResult AboveAverage() =>
            RunQueries(DocumentCubeQueries.ClientsAboveAverageRevenue, MdxMapper.ToClientsAboveAverage);

        /// <summary>Clients with outstanding unpaid balance (Doc Reste > 0).</summary>
        [HttpGet("outstanding-balance")]
        public IActionResult OutstandingBalance() =>
            RunQueries(DocumentCubeQueries.ClientsWithOutstandingBalance, MdxMapper.ToClientBalance);

        /// <summary>Clients with high discount AND negative margin — risk alert.</summary>
        [HttpGet("high-discount-low-margin")]
        public IActionResult HighDiscountLowMargin() =>
            RunQueries(DocumentCubeQueries.HighDiscountLowMarginClients, MdxMapper.ToClientDiscount);

        /// <summary>Top 15 clients by gross margin (HT - P Revient).</summary>
        [HttpGet("by-margin")]
        public IActionResult ByMargin() =>
            RunQueries(DocumentCubeQueries.GrossMarginByClient, MdxMapper.ToGrossMarginByClient);

        /// <summary>Top 20 clients by average order value (HT / number of docs).</summary>
        [HttpGet("avg-order-value")]
        public IActionResult AvgOrderValue() =>
            RunQueries(DocumentCubeQueries.AverageOrderValueByClient, MdxMapper.ToAvgOrderByClient);
    }
}
