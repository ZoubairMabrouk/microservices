using MdxServices.Interfaces;
using MdxServices.MDX;
using MdxServices.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Data;
using Xunit;

namespace MdxServices.Tests;

/// <summary>
/// Unit tests for MdxService — mocks IMdxQuery so no ADOMD connection is needed.
/// </summary>
public class MdxServiceTests
{
    private readonly Mock<IMdxQuery> _queryMock = new();
    private readonly Mock<ILogger<MdxService>> _loggerMock = new();

    private MdxService CreateService() => new(_queryMock.Object, _loggerMock.Object);

    private static DataTable BuildTable(params (string col, object value)[] rows)
    {
        var table = new DataTable();
        if (rows.Length == 0) return table;

        // Collect unique column names and infer types from values
        var cols = rows.Select(r => r.col).Distinct().ToList();
        foreach (var (col, value) in rows)
        {
            if (!table.Columns.Contains(col))
                table.Columns.Add(col, value.GetType());
        }

        var row = table.NewRow();
        foreach (var (col, value) in rows)
            row[col] = value;
        table.Rows.Add(row);

        return table;
    }

    [Fact]
    public void Execute_ReturnsMappedDictionaries()
    {
        var table = BuildTable(("Name", "Alice"), ("Amount", 100m));
        _queryMock.Setup(q => q.ExecuteMdxQuery(It.IsAny<string>())).Returns(table);

        var service = CreateService();
        var result = service.Execute("SELECT ...").ToList();

        Assert.Single(result);
        Assert.Equal("Alice", result[0]["Name"]);
        Assert.Equal(100m, result[0]["Amount"]);
    }

    [Fact]
    public void Execute_ReturnsEmptyList_WhenTableHasNoRows()
    {
        _queryMock.Setup(q => q.ExecuteMdxQuery(It.IsAny<string>()))
                  .Returns(new DataTable());

        var service = CreateService();
        var result = service.Execute("SELECT ...").ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Execute_MapsDbNullToNull()
    {
        var table = new DataTable();
        table.Columns.Add("Col1");
        var row = table.NewRow();
        row["Col1"] = DBNull.Value;
        table.Rows.Add(row);

        _queryMock.Setup(q => q.ExecuteMdxQuery(It.IsAny<string>())).Returns(table);

        var service = CreateService();
        var result = service.Execute("SELECT ...").ToList();

        Assert.Single(result);
        Assert.Null(result[0]["Col1"]);
    }

    [Fact]
    public void Execute_PassesQueryStringToUnderlyingQuery()
    {
        const string mdx = "SELECT [Measures] ON 0 FROM [Cube]";
        _queryMock.Setup(q => q.ExecuteMdxQuery(mdx)).Returns(new DataTable()).Verifiable();

        var service = CreateService();
        service.Execute(mdx);

        _queryMock.Verify(q => q.ExecuteMdxQuery(mdx), Times.Once);
    }

    [Fact]
    public void Execute_MultipleRows_ReturnsAll()
    {
        var table = new DataTable();
        table.Columns.Add("Year");
        table.Columns.Add("HT");
        table.Rows.Add("2022", 1000m);
        table.Rows.Add("2023", 2000m);

        _queryMock.Setup(q => q.ExecuteMdxQuery(It.IsAny<string>())).Returns(table);

        var service = CreateService();
        var result = service.Execute("SELECT ...").ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("2022", result[0]["Year"]);
        Assert.Equal("2023", result[1]["Year"]);
    }
}
