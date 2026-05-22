using MdxServices.Controllers;
using MdxServices.Interfaces;
using MdxServices.Models;
using MdxServices.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MdxServices.Tests.Controllers;

public class DiscountControllerTests
{
    private readonly Mock<IMdxService> _serviceMock = new();
    private readonly Mock<ILogger<DiscountController>> _loggerMock = new();

    private DiscountController CreateController() =>
        new(_serviceMock.Object, _loggerMock.Object);

    [Fact]
    public void RatioByYear_Returns200_WithData()
    {
        var data = new List<Dictionary<string, object?>>
        {
            new()
            {
                ["[Dim Date].[Year].[Year].[MEMBER_CAPTION]"] = "2023",
                ["[Measures].[Doc Total HT]"]     = 10000m,
                ["[Measures].[Doc Total Remise]"] = 500m,
                ["[Measures].[Discount Ratio %]"] = 0.05m
            }
        };

        _serviceMock.Setup(s => s.Execute(It.IsAny<string>())).Returns(data);

        var result = CreateController().RatioByYear() as OkObjectResult;
        var body = result!.Value as ApiResponse<IEnumerable<PeriodDto>>;

        Assert.True(body!.Success);
        Assert.Equal(1, body.RowCount);
    }

    [Fact]
    public void TopClientsByDiscount_Returns200_WithData()
    {
        var data = new List<Dictionary<string, object?>>
        {
            new()
            {
                ["[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]"] = "ACME",
                ["[Measures].[Doc Total Remise]"] = 800m,
                ["[Measures].[Doc Taux Rem]"]     = 0.08m
            }
        };

        _serviceMock.Setup(s => s.Execute(It.IsAny<string>())).Returns(data);

        var result = CreateController().TopClientsByDiscount() as OkObjectResult;
        var body = result!.Value as ApiResponse<IEnumerable<ClientDto>>;

        Assert.True(body!.Success);
        Assert.Equal(1, body.RowCount);
    }

    [Fact]
    public void ByDocumentType_Returns200_WithData()
    {
        var data = new List<Dictionary<string, object?>>
        {
            new()
            {
                ["[Document Dim].[Document].[Document].[MEMBER_CAPTION]"] = "FAC",
                ["[Measures].[Doc Total Remise]"] = 300m,
                ["[Measures].[Doc Val Rem]"]      = 25m
            }
        };

        _serviceMock.Setup(s => s.Execute(It.IsAny<string>())).Returns(data);

        var result = CreateController().ByDocumentType() as OkObjectResult;
        var body = result!.Value as ApiResponse<IEnumerable<DocumentDto>>;

        Assert.True(body!.Success);
        Assert.Equal(1, body.RowCount);
    }

    [Fact]
    public void RatioByYear_Returns500_OnUnexpectedException()
    {
        // AdomdErrorResponseException has no public constructor — test via generic Exception path.
        _serviceMock.Setup(s => s.Execute(It.IsAny<string>()))
                    .Throws(new InvalidOperationException("simulated MDX failure"));

        var result = CreateController().RatioByYear() as ObjectResult;

        Assert.Equal(500, result!.StatusCode);
        var body = result.Value as ApiResponse<object>;
        Assert.False(body!.Success);
    }
}
