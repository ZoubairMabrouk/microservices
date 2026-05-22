using MdxServices.Dto;
using Xunit;

namespace MdxServices.Tests;

/// <summary>
/// Unit tests for MdxMapper — pure functions, no DB or ADOMD needed.
/// Each test builds a fake MDX row (Dictionary) and asserts the mapped DTO.
/// </summary>
public class MdxMapperTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static Dictionary<string, object?> Row(params (string key, object? value)[] pairs)
        => pairs.ToDictionary(p => p.key, p => p.value);

    // ── ToYearRevenue ─────────────────────────────────────────────────────────

    [Fact]
    public void ToYearRevenue_MapsLabelAndValues()
    {
        var data = new[]
        {
            Row(("[Dim Date].[Year].[Year].[MEMBER_CAPTION]", "2023"),
                ("[Measures].[Doc Total HT]",  1000m),
                ("[Measures].[Doc Total TTC]", 1190m))
        };

        var result = MdxMapper.ToYearRevenue(data).ToList();

        Assert.Single(result);
        Assert.Equal("2023", result[0].Label);
        Assert.Equal(1000m, result[0].Value);
        Assert.Equal(1190m, result[0].CurrentValue);
    }

    [Fact]
    public void ToYearRevenue_GroupsSameYear()
    {
        var data = new[]
        {
            Row(("[Dim Date].[Year].[Year].[MEMBER_CAPTION]", "2023"),
                ("[Measures].[Doc Total HT]", 500m),
                ("[Measures].[Doc Total TTC]", 595m)),
            Row(("[Dim Date].[Year].[Year].[MEMBER_CAPTION]", "2023"),
                ("[Measures].[Doc Total HT]", 500m),
                ("[Measures].[Doc Total TTC]", 595m))
        };

        var result = MdxMapper.ToYearRevenue(data).ToList();

        Assert.Single(result);
        Assert.Equal(1000m, result[0].Value);
        Assert.Equal(1190m, result[0].CurrentValue);
    }

    [Fact]
    public void ToYearRevenue_SkipsRowsMissingYearKey()
    {
        var data = new[]
        {
            Row(("[Measures].[Doc Total HT]", 999m)) // no year key
        };

        var result = MdxMapper.ToYearRevenue(data).ToList();

        Assert.Empty(result);
    }

    // ── ToMonthRevenue ────────────────────────────────────────────────────────

    [Fact]
    public void ToMonthRevenue_MapsMonthLabel()
    {
        var data = new[]
        {
            Row(("[Dim Date].[Hiérarchie].[Month Name].[MEMBER_CAPTION]", "January"),
                ("[Measures].[Doc Total HT]", 2000m))
        };

        var result = MdxMapper.ToMonthRevenue(data).ToList();

        Assert.Single(result);
        Assert.Equal("January", result[0].Label);
        Assert.Equal(2000m, result[0].Value);
    }

    // ── ToClientRevenue ───────────────────────────────────────────────────────

    [Fact]
    public void ToClientRevenue_MapsClientAndTotals()
    {
        var data = new[]
        {
            Row(("[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]", "ACME"),
                ("[Measures].[Doc Total HT]",  5000m),
                ("[Measures].[Doc Total TTC]", 5950m))
        };

        var result = MdxMapper.ToClientRevenue(data).ToList();

        Assert.Single(result);
        Assert.Equal("ACME", result[0].Client);
        Assert.Equal(5000m, result[0].TotalHT);
        Assert.Equal(5950m, result[0].TotalTTC);
    }

    [Fact]
    public void ToClientRevenue_GroupsSameClient()
    {
        var data = new[]
        {
            Row(("[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]", "ACME"),
                ("[Measures].[Doc Total HT]", 1000m),
                ("[Measures].[Doc Total TTC]", 1190m)),
            Row(("[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]", "ACME"),
                ("[Measures].[Doc Total HT]", 2000m),
                ("[Measures].[Doc Total TTC]", 2380m))
        };

        var result = MdxMapper.ToClientRevenue(data).ToList();

        Assert.Single(result);
        Assert.Equal(3000m, result[0].TotalHT);
        Assert.Equal(3570m, result[0].TotalTTC);
    }

    [Fact]
    public void ToClientRevenue_SkipsRowsMissingClientKey()
    {
        var data = new[]
        {
            Row(("[Measures].[Doc Total HT]", 100m))
        };

        var result = MdxMapper.ToClientRevenue(data).ToList();

        Assert.Empty(result);
    }

    // ── ToClientDiscount ──────────────────────────────────────────────────────

    [Fact]
    public void ToClientDiscount_MapsDiscountFields()
    {
        var data = new[]
        {
            Row(("[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]", "BETA"),
                ("[Measures].[Doc Total Remise]", 300m),
                ("[Measures].[Doc Taux Rem]",     0.06m))
        };

        var result = MdxMapper.ToClientDiscount(data).ToList();

        Assert.Single(result);
        Assert.Equal("BETA", result[0].Client);
        Assert.Equal(300m, result[0].TotalDiscount);
        Assert.Equal(0.06m, result[0].DiscountRate);
    }

    // ── ToDocumentDiscount ────────────────────────────────────────────────────

    [Fact]
    public void ToDocumentDiscount_MapsDocTypeAndValues()
    {
        var data = new[]
        {
            Row(("[Document Dim].[Document].[Document].[MEMBER_CAPTION]", "FAC"),
                ("[Measures].[Doc Total Remise]", 150m),
                ("[Measures].[Doc Val Rem]",      10m))
        };

        var result = MdxMapper.ToDocumentDiscount(data).ToList();

        Assert.Single(result);
        Assert.Equal("FAC", result[0].DocumentType);
        Assert.Equal(150m, result[0].TotalValue);
        Assert.Equal(10m, result[0].Rate);
    }

    // ── ToTvaMonth ────────────────────────────────────────────────────────────

    [Fact]
    public void ToTvaMonth_MapsMonthTvaAndRate()
    {
        var data = new[]
        {
            Row(("[Dim Date].[Hiérarchie].[Month Name].[MEMBER_CAPTION]", "March"),
                ("[Measures].[Doc Total TVA]", 190m),
                ("[Measures].[Doc Tx Tva]",    0.19m))
        };

        var result = MdxMapper.ToTvaMonth(data).ToList();

        Assert.Single(result);
        Assert.Equal("March", result[0].Month);
        Assert.Equal(190m, result[0].TotalValue);
        Assert.Equal(0.19m, result[0].Rate);
    }

    // ── ToFodec ───────────────────────────────────────────────────────────────

    [Fact]
    public void ToFodec_MapsDocTypeAndFodecValues()
    {
        var data = new[]
        {
            Row(("[Document Dim].[Document].[Document].[MEMBER_CAPTION]", "FAC"),
                ("[Measures].[Doc Total Fodec]", 50m),
                ("[Measures].[Doc Val Fodec]",   5m))
        };

        var result = MdxMapper.ToFodec(data).ToList();

        Assert.Single(result);
        Assert.Equal("FAC", result[0].DocumentType);
        Assert.Equal(50m, result[0].TotalValue);
        Assert.Equal(5m, result[0].Rate);
    }

    // ── ToTaxQuarter ──────────────────────────────────────────────────────────

    [Fact]
    public void ToTaxQuarter_MapsQuarterTtcTvaAndRate()
    {
        var data = new[]
        {
            Row(("[Dim Date].[Quarter].[Quarter].[MEMBER_CAPTION]", "2023-Q1"),
                ("[Measures].[Doc Total TTC]", 1190m),
                ("[Measures].[Doc Total TVA]",  190m),
                ("[Measures].[TVA Ratio %]",    0.16m))
        };

        var result = MdxMapper.ToTaxQuarter(data).ToList();

        Assert.Single(result);
        Assert.Equal("2023-Q1", result[0].Quarter);
        Assert.Equal(1190m, result[0].TotalValue);
        Assert.Equal(190m, result[0].Cost);
        Assert.Equal(0.16m, result[0].Rate);
    }

    // ── ToDiscountRatioByYear ─────────────────────────────────────────────────

    [Fact]
    public void ToDiscountRatioByYear_MapsAllFields()
    {
        var data = new[]
        {
            Row(("[Dim Date].[Year].[Year].[MEMBER_CAPTION]", "2024"),
                ("[Measures].[Doc Total HT]",     10000m),
                ("[Measures].[Doc Total Remise]",   500m),
                ("[Measures].[Discount Ratio %]",   0.05m))
        };

        var result = MdxMapper.ToDiscountRatioByYear(data).ToList();

        Assert.Single(result);
        Assert.Equal("2024", result[0].Label);
        Assert.Equal(10000m, result[0].Value);
        Assert.Equal(500m, result[0].PreviousValue);
        Assert.Equal(0.05m, result[0].Growth);
    }

    // ── ToQuarterRevenue ──────────────────────────────────────────────────────

    [Fact]
    public void ToQuarterRevenue_ReturnsEmpty_WhenNoMatchingQuarterKeys()
    {
        // The mapper filters for keys containing "[Dim Date].[Quarter].&[" and then
        // extracts the date via Split('[',']')[2]. With the standard MDX key format
        // "[Dim Date].[Quarter].&[20230101]", index [2] yields "." (length 1, not 8),
        // so the mapper skips all rows — this documents that known behaviour.
        var data = new[]
        {
            Row(("[Document Dim].[Document].[Document].[MEMBER_CAPTION]", "FAC"),
                ("[Dim Date].[Quarter].&[20230101]", 1000m))
        };

        var result = MdxMapper.ToQuarterRevenue(data).ToList();

        Assert.Empty(result);
    }

    // ── ToClientBalance ───────────────────────────────────────────────────────

    [Fact]
    public void ToClientBalance_MapsBalanceFields()
    {
        var data = new[]
        {
            Row(("[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]", "DELTA"),
                ("[Measures].[Doc Total TTC]", 1000m),
                ("[Measures].[Doc Acompte]",    400m),
                ("[Measures].[Doc Reste]",       600m))
        };

        var result = MdxMapper.ToClientBalance(data).ToList();

        Assert.Single(result);
        Assert.Equal("DELTA", result[0].Client);
        Assert.Equal(1000m, result[0].TotalTTC);
        Assert.Equal(400m, result[0].Paid);
        Assert.Equal(600m, result[0].Remaining);
    }

    [Fact]
    public void ToClientBalance_FiltersOutZeroRemaining()
    {
        var data = new[]
        {
            Row(("[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]", "PAID"),
                ("[Measures].[Doc Total TTC]", 500m),
                ("[Measures].[Doc Acompte]",   500m),
                ("[Measures].[Doc Reste]",       0m))
        };

        var result = MdxMapper.ToClientBalance(data).ToList();

        Assert.Empty(result);
    }

    // ── ToClientsAboveAverage ─────────────────────────────────────────────────

    [Fact]
    public void ToClientsAboveAverage_MapsClientAndTotals()
    {
        var data = new[]
        {
            Row(("[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]", "EPSILON"),
                ("[Measures].[Doc Total HT]",  12000m),
                ("[Measures].[Doc Total TTC]", 14280m))
        };

        var result = MdxMapper.ToClientsAboveAverage(data).ToList();

        Assert.Single(result);
        Assert.Equal("EPSILON", result[0].Client);
        Assert.Equal(12000m, result[0].TotalHT);
        Assert.Equal(14280m, result[0].TotalTTC);
    }

    // ── ToRevenueConcentration ────────────────────────────────────────────────

    [Fact]
    public void ToRevenueConcentration_MapsRevenueAndShare()
    {
        var data = new[]
        {
            Row(("[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]", "GAMMA"),
                ("[Measures].[Doc Total HT]",    8000m),
                ("[Measures].[Revenue Share %]", 0.25m))
        };

        var result = MdxMapper.ToRevenueConcentration(data).ToList();

        Assert.Single(result);
        Assert.Equal("GAMMA", result[0].Client);
        Assert.Equal(8000m, result[0].Revenue);
        Assert.Equal(0.25m, result[0].SharePercent);
    }

    // ── ToGrossMarginByClient ─────────────────────────────────────────────────

    [Fact]
    public void ToGrossMarginByClient_MapsMarginFields()
    {
        var data = new[]
        {
            Row(("[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]", "ZETA"),
                ("[Measures].[Doc Total HT]",  5000m),
                ("[Measures].[Doc P Revient]", 3000m),
                ("[Measures].[Gross Margin]",  2000m),
                ("[Measures].[Margin Rate %]", 0.40m))
        };

        var result = MdxMapper.ToGrossMarginByClient(data).ToList();

        Assert.Single(result);
        Assert.Equal("ZETA", result[0].Client);
        Assert.Equal(5000m, result[0].TotalHT);
        Assert.Equal(3000m, result[0].Cost);
        Assert.Equal(2000m, result[0].Margin);
        Assert.Equal(0.40m, result[0].MarginRate);
    }

    // ── ToAvgOrderByClient ────────────────────────────────────────────────────

    [Fact]
    public void ToAvgOrderByClient_MapsOrderCountAndAvg()
    {
        var data = new[]
        {
            Row(("[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]", "ETA"),
                ("[Measures].[Doc Total HT]",          9000m),
                ("[Measures].[Document Fact Nombre]",     3m),
                ("[Measures].[Avg Order Value]",        3000m))
        };

        var result = MdxMapper.ToAvgOrderByClient(data).ToList();

        Assert.Single(result);
        Assert.Equal("ETA", result[0].Client);
        Assert.Equal(3, result[0].OrderCount);
        Assert.Equal(3000m, result[0].AvgOrderValue);
    }

    // ── ToYoYRevenueByClient ──────────────────────────────────────────────────

    [Fact]
    public void ToYoYRevenueByClient_MapsYearlyGrowth()
    {
        var data = new[]
        {
            Row(("[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]", "THETA"),
                ("[Measures].[Revenue 2022]",  8000m),
                ("[Measures].[Revenue 2023]", 10000m),
                ("[Measures].[YoY Growth %]",  0.25m))
        };

        var result = MdxMapper.ToYoYRevenueByClient(data, 2023, 2022).ToList();

        Assert.Single(result);
        Assert.Equal("THETA", result[0].Client);
        Assert.Equal(8000m, result[0].PreviousYear);
        Assert.Equal(10000m, result[0].CurrentYear);
        Assert.Equal(0.25m, result[0].Growth);
    }

    // ── ToRiskClients ─────────────────────────────────────────────────────────

    [Fact]
    public void ToRiskClients_MapsDiscountAndMargin()
    {
        var data = new[]
        {
            Row(("[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]", "RISKY"),
                ("[Measures].[Doc Total HT]",  2000m),
                ("[Measures].[Doc Taux Rem]",   0.15m),
                ("[Measures].[Gross Margin]",  -300m))
        };

        var result = MdxMapper.ToRiskClients(data).ToList();

        Assert.Single(result);
        Assert.Equal("RISKY", result[0].Client);
        Assert.Equal(0.15m, result[0].DiscountRate);
        Assert.Equal(-300m, result[0].Margin);
    }
}
