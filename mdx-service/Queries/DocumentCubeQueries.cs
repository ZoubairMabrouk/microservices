namespace MdxServices.queries
{
    public class DocumentCubeQueries
    {
        // ── Revenue ─────────────────────────────────────────────────────────────

        public static string RevenueByYear => @"
            SELECT
                {[Measures].[Doc Total HT], [Measures].[Doc Total TTC]} ON COLUMNS,
                NON EMPTY [Dim Date].[Year].MEMBERS ON ROWS
            FROM [Dw-ing-pfe]";

        public static string RevenueByMonth(int year) => $@"
            SELECT
    [Measures].[Doc Total HT] ON COLUMNS,
    NON EMPTY
    DESCENDANTS(
        FILTER(
            [Dim Date].[Hiérarchie].[Year].MEMBERS,
            LEFT([Dim Date].[Hiérarchie].CURRENTMEMBER.NAME, 4) = ""{year}""
        ),
        [Dim Date].[Hiérarchie].[Month Name]
    ) ON ROWS
FROM [Dw-ing-pfe]";

        public static string RevenueByQuarterAndDocType => @"
            SELECT
                NON EMPTY [Dim Date].[Quarter].MEMBERS ON COLUMNS,
                NON EMPTY [Document Dim].[Document].MEMBERS ON ROWS
            FROM [Dw-ing-pfe]
            WHERE [Measures].[Doc Total HT]";

        public static string RevenueByClientDesc => @"
            SELECT
                [Measures].[Doc Total TTC] ON COLUMNS,
                ORDER(
                    NONEMPTY ([Tiers Dim].[Tiers].[Tiers].MEMBERS,
                    [Measures].[Doc Total TTC]),
                    [Measures].[Doc Total TTC],BDESC
                ) ON ROWS
            FROM [Dw-ing-pfe]";

        public static string Top10ClientsByRevenue => @"
            SELECT
                {[Measures].[Doc Total HT], [Measures].[Doc Total TTC]} ON COLUMNS,
                TOPCOUNT(
                    NONEMPTY ([Tiers Dim].[Tiers].[Tiers].MEMBERS, [Measures].[Doc Total TTC]),
                    10,
                    [Measures].[Doc Total TTC]
                ) ON ROWS
            FROM [Dw-ing-pfe]";

        public static string Bottom10ClientsByRevenue => @"
            SELECT
                [Measures].[Doc Total TTC] ON COLUMNS,
                BOTTOMCOUNT(
                    NONEMPTY ([Tiers Dim].[Tiers].[Tiers].MEMBERS, [Measures].[Doc Total TTC]),
                    10,
                    [Measures].[Doc Total TTC]
                ) ON ROWS
            FROM [Dw-ing-pfe]";

        // ── Discounts ────────────────────────────────────────────────────────────

        public static string DiscountRatioByYear => @"
            WITH
                MEMBER [Measures].[Discount Ratio %] AS
                    IIF(
                        [Measures].[Doc Total HT] = 0, NULL,
                        ([Measures].[Doc Total Remise] / [Measures].[Doc Total HT]) * 100
                    ), FORMAT_STRING = '0.00'
            SELECT
                {[Measures].[Doc Total HT],
                 [Measures].[Doc Total Remise],
                 [Measures].[Discount Ratio %]} ON COLUMNS,
                NON EMPTY [Dim Date].[Year].MEMBERS ON ROWS
            FROM [Dw-ing-pfe]";

        public static string Top10ClientsByDiscount => @"
            SELECT
                {[Measures].[Doc Total Remise], [Measures].[Doc Taux Rem]} ON COLUMNS,
                TOPCOUNT(
                    NONEMPTY ([Tiers Dim].[Tiers].[Tiers].MEMBERS, [Measures].[Doc Total Remise]),
                    10,
                    [Measures].[Doc Total Remise]
                ) ON ROWS
            FROM [Dw-ing-pfe]";

        public static string DiscountByDocumentType => @"
            SELECT
                {[Measures].[Doc Total Remise], [Measures].[Doc Val Rem]} ON COLUMNS,
                NON EMPTY [Document Dim].[Document].MEMBERS ON ROWS
            FROM [Dw-ing-pfe]";

        // ── Tax / Compliance ─────────────────────────────────────────────────────

        public static string TvaByMonth(int year) => $@"
            SELECT
                {{[Measures].[Doc Total TVA], [Measures].[Doc Tx Tva]}} ON COLUMNS,
                NON EMPTY [Dim Date].[Month Name].MEMBERS ON ROWS
            FROM [Dw-ing-pfe]
            WHERE [Dim Date].[Year].[{year}]";

        public static string FodecByDocumentType => @"
            SELECT
                {[Measures].[Doc Total Fodec], [Measures].[Doc Val Fodec]} ON COLUMNS,
                NON EMPTY [Document Dim].[Document].MEMBERS ON ROWS
            FROM [Dw-ing-pfe]";

        public static string TaxBurdenRatioByQuarter => @"
            WITH
                MEMBER [Measures].[TVA Ratio %] AS
                    IIF(
                        [Measures].[Doc Total TTC] = 0, NULL,
                        ([Measures].[Doc Total TVA] / [Measures].[Doc Total TTC]) * 100
                    ), FORMAT_STRING = '0.00'
            SELECT
                {[Measures].[Doc Total TTC],
                 [Measures].[Doc Total TVA],
                 [Measures].[TVA Ratio %]} ON COLUMNS,
                NON EMPTY [Dim Date].[Quarter].MEMBERS ON ROWS
            FROM [Dw-ing-pfe]";

        // ── Year-over-Year ───────────────────────────────────────────────────────

        public static string YoYRevenueByClient(int currentYear, int priorYear) => $@"
            WITH
                MEMBER [Measures].[Revenue {currentYear}] AS
                    ([Dim Date].[Year].[{currentYear}], [Measures].[Doc Total HT])
                MEMBER [Measures].[Revenue {priorYear}] AS
                    ([Dim Date].[Year].[{priorYear}], [Measures].[Doc Total HT])
                MEMBER [Measures].[YoY Growth %] AS
                    IIF(
                        [Measures].[Revenue {priorYear}] = 0, NULL,
                        (([Measures].[Revenue {currentYear}] - [Measures].[Revenue {priorYear}])
                            / [Measures].[Revenue {priorYear}]) * 100
                    ), FORMAT_STRING = '0.00'
            SELECT
                {{[Measures].[Revenue {priorYear}],
                  [Measures].[Revenue {currentYear}],
                  [Measures].[YoY Growth %]}} ON COLUMNS,
                NON EMPTY [Tiers Dim].[Tiers].MEMBERS ON ROWS
            FROM [Dw-ing-pfe]";

        public static string YoYVolumeByQuarter(int currentYear, int priorYear) => $@"
            WITH
                MEMBER [Measures].[Qty {currentYear}] AS
                    ([Dim Date].[Year].[{currentYear}], [Measures].[Doc Qte])
                MEMBER [Measures].[Qty {priorYear}] AS
                    ([Dim Date].[Year].[{priorYear}], [Measures].[Doc Qte])
                MEMBER [Measures].[Volume Growth %] AS
                    IIF(
                        [Measures].[Qty {priorYear}] = 0, NULL,
                        (([Measures].[Qty {currentYear}] - [Measures].[Qty {priorYear}])
                            / [Measures].[Qty {priorYear}]) * 100
                    ), FORMAT_STRING = '0.00'
            SELECT
                {{[Measures].[Qty {priorYear}],
                  [Measures].[Qty {currentYear}],
                  [Measures].[Volume Growth %]}} ON COLUMNS,
                NON EMPTY [Dim Date].[Quarter].MEMBERS ON ROWS
            FROM [Dw-ing-pfe]";

        // ── YTD / QTD ────────────────────────────────────────────────────────────

        public static string YtdRevenueByMonth(int year) => $@"
            WITH MEMBER [Measures].[YTD Revenue HT] AS
                SUM(YTD([Dim Date].[dmn].CURRENTMEMBER), [Measures].[Doc Total HT])
            SELECT
                [Measures].[YTD Revenue HT] ON COLUMNS,
                NON EMPTY [Dim Date].[Month Name].MEMBERS ON ROWS
            FROM [Dw-ing-pfe]
            WHERE [Dim Date].[Year].[{year}]";

        public static string QtdRevenueByMonth(int year) => $@"
            WITH MEMBER [Measures].[QTD Revenue HT] AS
                SUM(
                    PERIODSTODATE([Dim Date].[dmn].[Quarter], [Dim Date].[dmn].CURRENTMEMBER),
                    [Measures].[Doc Total HT]
                )
            SELECT
                [Measures].[QTD Revenue HT] ON COLUMNS,
                NON EMPTY [Dim Date].[Month Name].MEMBERS ON ROWS
            FROM [Dw-ing-pfe]
            WHERE [Dim Date].[Year].[{year}]";

        // ── Margin / Cost ─────────────────────────────────────────────────────────

        public static string GrossMarginByClient => @"
            WITH
    MEMBER [Measures].[Gross Margin] AS
        [Measures].[Doc Total HT] - [Measures].[Doc P Revient]
    MEMBER [Measures].[Margin Rate %] AS
        IIF(
            [Measures].[Doc Total HT] = 0,
            NULL,
            ([Measures].[Gross Margin] / [Measures].[Doc Total HT]) * 100
        ), FORMAT_STRING = '0.00'
SELECT
    {[Measures].[Doc Total HT],
      [Measures].[Doc P Revient],
      [Measures].[Gross Margin],
      [Measures].[Margin Rate %]} ON COLUMNS,
    TOPCOUNT(
        NONEMPTY([Tiers Dim].[Tiers].MEMBERS, [Measures].[Gross Margin]),
        15,
        [Measures].[Gross Margin]
    ) ON ROWS
FROM [Dw-ing-pfe]";

        public static string MonthlyMarginTrend(int year) => $@"
            WITH
    MEMBER [Measures].[Gross Margin] AS
        [Measures].[Doc Total HT] - [Measures].[Doc P Revient]
SELECT
    {{
        [Measures].[Doc Total HT],
        [Measures].[Doc P Revient],
        [Measures].[Gross Margin]
    }} ON COLUMNS,
    NON EMPTY
    DESCENDANTS(
        FILTER(
            [Dim Date].[Hiérarchie].[Year].MEMBERS,
            LEFT([Dim Date].[Hiérarchie].CURRENTMEMBER.NAME, 4) = ""{year}""
        ),
        [Dim Date].[Hiérarchie].[Month Name]
    ) ON ROWS
FROM [Dw-ing-pfe]";

        // ── Client Segmentation ───────────────────────────────────────────────────

        public static string ClientsAboveAverageRevenue => @"
            SELECT
                {[Measures].[Doc Total HT], [Measures].[Doc Total TTC]} ON COLUMNS,
                NON EMPTY
                FILTER(
                    [Tiers Dim].[Tiers].MEMBERS,
                    [Measures].[Doc Total HT] >
                        AVG([Tiers Dim].[Tiers].MEMBERS, [Measures].[Doc Total HT])
                ) ON ROWS
            FROM [Dw-ing-pfe]";

        public static string ClientsWithOutstandingBalance => @"
            SELECT
                {[Measures].[Doc Total TTC],
                 [Measures].[Doc Acompte],
                 [Measures].[Doc Reste]} ON COLUMNS,
                ORDER(
                    NONEMPTY(
                        [Tiers Dim].[Tiers].MEMBERS,
                        [Measures].[Doc Reste]
                    ),
                    [Measures].[Doc Reste],
                    BDESC
                ) ON ROWS
            FROM [Dw-ing-pfe]";

        public static string HighDiscountLowMarginClients => @"
            WITH
                MEMBER [Measures].[Gross Margin] AS
                    [Measures].[Doc Total HT] - [Measures].[Doc P Revient]
            SELECT
                {[Measures].[Doc Total HT],
                 [Measures].[Doc Taux Rem],
                 [Measures].[Gross Margin]} ON COLUMNS,
                NON EMPTY
                FILTER(
                    [Tiers Dim].[Tiers].MEMBERS,
                    [Measures].[Doc Taux Rem] > 10 AND [Measures].[Gross Margin] < 0
                ) ON ROWS
            FROM [Dw-ing-pfe]";

        // ── Documents & Volume ────────────────────────────────────────────────────

        public static string AverageOrderValueByClient => @"
            WITH 
  MEMBER [Measures].[Avg Order Value] AS
    IIF(
        [Measures].[Document Fact Nombre] = 0,
        NULL,
        [Measures].[Doc Total HT] / [Measures].[Document Fact Nombre]
    ),
    FORMAT_STRING = '#,##0.00'
SELECT
  {[Measures].[Doc Total HT], [Measures].[Document Fact Nombre], [Measures].[Avg Order Value]} ON COLUMNS,
  TOPCOUNT(
    FILTER([Tiers Dim].[Tiers].MEMBERS, NOT ISEMPTY([Measures].[Avg Order Value])),
    20,
    [Measures].[Avg Order Value]
  ) ON ROWS
FROM [Dw-ing-pfe]";

        public static string VolumeByDocTypeAndQuarter => @"
            SELECT
                NON EMPTY [Dim Date].[Quarter].MEMBERS ON COLUMNS,
                NON EMPTY CROSSJOIN(
                    [Document Dim].[Document].MEMBERS,
                    [Document Dim].[Type AV].MEMBERS
                ) ON ROWS
            FROM [Dw-ing-pfe]
            WHERE [Measures].[Doc Qte]";

        public static string RevenueConcentrationTop20Pct => @"
            WITH
                MEMBER [Measures].[Total Revenue All] AS
                    SUM([Tiers Dim].[Tiers].MEMBERS, [Measures].[Doc Total HT])
                MEMBER [Measures].[Revenue Share %] AS
                    IIF(
                        [Measures].[Total Revenue All] = 0, NULL,
                        ([Measures].[Doc Total HT] / [Measures].[Total Revenue All]) * 100
                    ), FORMAT_STRING = '0.00'
            SELECT
                {[Measures].[Doc Total HT], [Measures].[Revenue Share %]} ON COLUMNS,
                TOPCOUNT(
                    NONEMPTY ([Tiers Dim].[Tiers].MEMBERS, [Measures].[Doc Total HT]),
                    20,
                    [Measures].[Doc Total HT]
                ) ON ROWS
            FROM [Dw-ing-pfe]";
    }
}
