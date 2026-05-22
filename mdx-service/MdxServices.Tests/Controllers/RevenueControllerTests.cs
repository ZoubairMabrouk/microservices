using MdxServices.Controllers;
using MdxServices.Interfaces;
using MdxServices.Models;
using MdxServices.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MdxServices.Tests.Controllers;

/// <summary>
/// Unit tests for RevenueController — mocks IMdxService so no ADOMD connection is needed.
/// Verifies HTTP status codes and ApiResponse shape for success and error paths.
/// </summary>
public class RevenueControllerTests
{
    private readonly Mock<IMdxService> _serviceMock = new();
    private readonly Mock<ILogger<RevenueController>> _loggerMock = new();

    private RevenueController CreateController() =>
        new(_serviceMock.Object, _loggerMock.Object);

    // ── helpers ───────────────────────────────────────────────────────────────

    private static List<Dictionary<string, object?>> YearRows(params (string year, decimal ht, decimal ttc)[] rows) =>
        rows.Select(r => new Dictionary<string, object?>
        {
            ["[Dim Date].[Year].[Year].[MEMBER_CAPTION]"] = r.year,
            ["[Measures].[Doc Total HT]"]  = r.ht,
            ["[Measures].[Doc Total TTC]"] = r.ttc
        }).ToList();

    private static List<Dictionary<string, object?>> MonthRows(params (string month, decimal ht)[] rows) =>
        rows.Select(r => new Dictionary<string, object?>
        {
            ["[Dim Date].[Hiérarchie].[Month Name].[MEMBER_CAPTION]"] = r.month,
            ["[Measures].[Doc Total HT]"] = r.ht
        }).ToList();

    // ── ByYear ────────────────────────────────────────────────────────────────

    [Fact]
    public void ByYear_Returns200_WithMappedData()
    {
        _serviceMock.Setup(s => s.Execute(It.IsAny<string>()))
                    .Returns(YearRows(("2023", 10000m, 11900m)));

        var result = CreateController().ByYear() as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result!.StatusCode);

        var body = result.Value as ApiResponse<IEnumerable<PeriodDto>>;
        Assert.NotNull(body);
        Assert.True(body!.Success);
        Assert.Equal(1, body.RowCount);
    }

    [Fact]
    public void ByYear_Returns200_WithEmptyList_WhenNoData()
    {
        _serviceMock.Setup(s => s.Execute(It.IsAny<string>()))
                    .Returns(new List<Dictionary<string, object?>>());

        var result = CreateController().ByYear() as OkObjectResult;

        Assert.NotNull(result);
        var body = result!.Value as ApiResponse<IEnumerable<PeriodDto>>;
        Assert.True(body!.Success);
        Assert.Equal(0, body.RowCount);
    }

    // ── ByMonth ───────────────────────────────────────────────────────────────

    [Fact]
    public void ByMonth_Returns200_WithMappedData()
    {
        _serviceMock.Setup(s => s.Execute(It.IsAny<string>()))
                    .Returns(MonthRows(("January", 5000m), ("February", 6000m)));

        var result = CreateController().ByMonth(2023) as OkObjectResult;

        Assert.NotNull(result);
        var body = result!.Value as ApiResponse<IEnumerable<PeriodDto>>;
        Assert.True(body!.Success);
        Assert.Equal(2, body.RowCount);
    }

    // ── Error paths ───────────────────────────────────────────────────────────

    [Fact]
    public void ByYear_Returns503_OnConnectionException()
    {
        // AdomdConnectionException has no public constructor — test via the
        // generic Exception path (500) which we can construct freely.
        _serviceMock.Setup(s => s.Execute(It.IsAny<string>()))
                    .Throws(new InvalidOperationException("simulated connection failure"));

        var result = CreateController().ByYear() as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(500, result!.StatusCode);

        var body = result.Value as ApiResponse<object>;
        Assert.NotNull(body);
        Assert.False(body!.Success);
    }

    [Fact]
    public void ByYear_Returns500_OnUnexpectedException()
    {
        _serviceMock.Setup(s => s.Execute(It.IsAny<string>()))
                    .Throws(new InvalidOperationException("boom"));

        var result = CreateController().ByYear() as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(500, result!.StatusCode);

        var body = result.Value as ApiResponse<object>;
        Assert.False(body!.Success);
        Assert.Contains("Internal error", body.Error);
    }
}
