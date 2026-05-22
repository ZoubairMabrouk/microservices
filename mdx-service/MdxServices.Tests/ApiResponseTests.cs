using MdxServices.Models;
using Xunit;

namespace MdxServices.Tests;

public class ApiResponseTests
{
    [Fact]
    public void Ok_SetsSuccessTrue_AndData()
    {
        var response = ApiResponse<string>.Ok("hello", 1);

        Assert.True(response.Success);
        Assert.Equal("hello", response.Data);
        Assert.Equal(1, response.RowCount);
        Assert.Null(response.Error);
    }

    [Fact]
    public void Fail_SetsSuccessFalse_AndError()
    {
        var response = ApiResponse<string>.Fail("something went wrong");

        Assert.False(response.Success);
        Assert.Equal("something went wrong", response.Error);
        Assert.Null(response.Data);
        Assert.Equal(0, response.RowCount);
    }

    [Fact]
    public void Ok_WithList_ReturnsCorrectRowCount()
    {
        var data = new List<int> { 1, 2, 3 };
        var response = ApiResponse<IEnumerable<int>>.Ok(data, data.Count);

        Assert.True(response.Success);
        Assert.Equal(3, response.RowCount);
    }
}
