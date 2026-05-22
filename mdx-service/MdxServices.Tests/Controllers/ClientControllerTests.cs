using MdxServices.Controllers;
using MdxServices.Interfaces;
using MdxServices.Models;
using MdxServices.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MdxServices.Tests.Controllers;

public class ClientControllerTests
{
    private readonly Mock<IMdxService> _serviceMock = new();
    private readonly Mock<ILogger<ClientController>> _loggerMock = new();

    private ClientController CreateController() =>
        new(_serviceMock.Object, _loggerMock.Object);

    private static List<Dictionary<string, object?>> ClientRows(params (string client, decimal ht, decimal ttc)[] rows) =>
        rows.Select(r => new Dictionary<string, object?>
        {
            ["[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]"] = r.client,
            ["[Measures].[Doc Total HT]"]  = r.ht,
            ["[Measures].[Doc Total TTC]"] = r.ttc
        }).ToList();

    [Fact]
    public void AboveAverage_Returns200_WithClients()
    {
        _serviceMock.Setup(s => s.Execute(It.IsAny<string>()))
                    .Returns(ClientRows(("ACME", 50000m, 59500m)));

        var result = CreateController().AboveAverage() as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result!.StatusCode);

        var body = result.Value as ApiResponse<IEnumerable<ClientDto>>;
        Assert.True(body!.Success);
        Assert.Equal(1, body.RowCount);
    }

    [Fact]
    public void OutstandingBalance_Returns200_OnlyClientsWithRemainingBalance()
    {
        var data = new List<Dictionary<string, object?>>
        {
            new()
            {
                ["[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]"] = "UNPAID",
                ["[Measures].[Doc Total TTC]"] = 1000m,
                ["[Measures].[Doc Acompte]"]   = 200m,
                ["[Measures].[Doc Reste]"]     = 800m
            },
            new()
            {
                ["[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]"] = "PAID",
                ["[Measures].[Doc Total TTC]"] = 500m,
                ["[Measures].[Doc Acompte]"]   = 500m,
                ["[Measures].[Doc Reste]"]     = 0m
            }
        };

        _serviceMock.Setup(s => s.Execute(It.IsAny<string>())).Returns(data);

        var result = CreateController().OutstandingBalance() as OkObjectResult;
        var body = result!.Value as ApiResponse<IEnumerable<ClientDto>>;

        Assert.True(body!.Success);
        Assert.Equal(1, body.RowCount); // only UNPAID passes the Remaining > 0 filter
    }

    [Fact]
    public void ByMargin_Returns200_WithMarginData()
    {
        var data = new List<Dictionary<string, object?>>
        {
            new()
            {
                ["[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]"] = "BETA",
                ["[Measures].[Doc Total HT]"]  = 8000m,
                ["[Measures].[Doc P Revient]"] = 5000m,
                ["[Measures].[Gross Margin]"]  = 3000m,
                ["[Measures].[Margin Rate %]"] = 0.375m
            }
        };

        _serviceMock.Setup(s => s.Execute(It.IsAny<string>())).Returns(data);

        var result = CreateController().ByMargin() as OkObjectResult;
        var body = result!.Value as ApiResponse<IEnumerable<ClientDto>>;

        Assert.True(body!.Success);
        Assert.Equal(1, body.RowCount);
    }

    [Fact]
    public void AvgOrderValue_Returns200_WithOrderData()
    {
        var data = new List<Dictionary<string, object?>>
        {
            new()
            {
                ["[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]"] = "GAMMA",
                ["[Measures].[Doc Total HT]"]          = 9000m,
                ["[Measures].[Document Fact Nombre]"]  = 3m,
                ["[Measures].[Avg Order Value]"]       = 3000m
            }
        };

        _serviceMock.Setup(s => s.Execute(It.IsAny<string>())).Returns(data);

        var result = CreateController().AvgOrderValue() as OkObjectResult;
        var body = result!.Value as ApiResponse<IEnumerable<ClientDto>>;

        Assert.True(body!.Success);
        Assert.Equal(1, body.RowCount);
    }

    [Fact]
    public void HighDiscountLowMargin_Returns500_OnUnexpectedException()
    {
        // AdomdConnectionException has no public constructor — test via generic Exception path.
        _serviceMock.Setup(s => s.Execute(It.IsAny<string>()))
                    .Throws(new InvalidOperationException("simulated failure"));

        var result = CreateController().HighDiscountLowMargin() as ObjectResult;

        Assert.Equal(500, result!.StatusCode);
        var body = result.Value as ApiResponse<object>;
        Assert.False(body!.Success);
    }
}
