//    namespace MdxServices.Dto
//    {
//    public static class MdxMapper
//    {
//        public static IEnumerable<PeriodDto> ToYearRevenue(IEnumerable<Dictionary<string, object?>> data) =>
//    ToPeriodRevenue(data, "[Dim Date].[Year].[Year].[MEMBER_CAPTION]", "[Measures].[Doc Total HT]");

//        public static IEnumerable<PeriodDto> ToMonthRevenue(IEnumerable<Dictionary<string, object?>> data) =>
//            ToPeriodRevenue(data, "[Dim Date].[Month].[Month].[MEMBER_CAPTION]", "[Measures].[Doc Total HT]");
//        // ── Generic Period Mapping (Year, Month, Quarter) ─────────────────────────────
//        public static IEnumerable<PeriodDto> ToQuarterRevenue(IEnumerable<Dictionary<string, object?>> data)
//        {
//            var result = new Dictionary<string, decimal>();

//            foreach (var row in data)
//            {
//                // Récupérer le type de document si nécessaire
//                var docType = row.ContainsKey("[Document Dim].[Document].[Document].[MEMBER_CAPTION]")
//                    ? row["[Document Dim].[Document].[Document].[MEMBER_CAPTION]"]?.ToString()
//                    : "";

//                // Parcourir toutes les clés qui correspondent aux quarts
//                foreach (var key in row.Keys.Where(k => k.Contains("[Dim Date].[Quarter].&[")))
//                {
//                    var raw = key.Split('[', ']')[2]; // ex: 20230101
//                    if (raw.Length != 8) continue;

//                    var year = raw.Substring(0, 4);
//                    var month = int.Parse(raw.Substring(4, 2));
//                    var quarter = $"{year}-Q{Math.Ceiling(month / 3.0)}";

//                    var value = Convert.ToDecimal(row[key] ?? 0);

//                    // Tu peux décider comment traiter les différents types de documents
//                    // Par exemple ici on considère FAC/FC comme positif et AVOIR comme négatif
//                    if (docType == "AVOIR") value = -value;

//                    if (!result.ContainsKey(quarter))
//                        result[quarter] = 0;

//                    result[quarter] += value;
//                }
//            }

//            return result
//                .Select(kv => new PeriodDto
//                {
//                    Label = kv.Key,
//                    Value = kv.Value
//                })
//                .OrderBy(x => x.Label);
//        }
//        public static IEnumerable<PeriodDto> ToPeriodRevenue(IEnumerable<Dictionary<string, object?>> data, string periodKey, string measureKey)
//        {
//            return data
//                .Select(d => new
//                {
//                    Label = d.ContainsKey(periodKey) ? d[periodKey]?.ToString() : null,
//                    Value = Convert.ToDecimal(d.ContainsKey(measureKey) ? d[measureKey] ?? 0 : 0)
//                })
//                .Where(x => !string.IsNullOrEmpty(x.Label))
//                .GroupBy(x => x.Label)
//                .Select(g => new PeriodDto
//                {
//                    Label = g.Key!,
//                    Value = g.Sum(x => x.Value)
//                })
//                .OrderBy(x => x.Label);
//        }

//        // ── Client Mapping ─────────────────────────────────────────────────────────────
//        public static IEnumerable<ClientDto> ToClientRevenue(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data
//                .Where(d => d.ContainsKey("[Dim Customer].[Customer].[Customer].[MEMBER_CAPTION]"))
//                .Select(d => new ClientDto
//                {
//                    Client = d["[Dim Customer].[Customer].[Customer].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                    TotalHT = Convert.ToDecimal(d["[Measures].[Doc Total HT]"] ?? 0),
//                    TotalTTC = Convert.ToDecimal(d["[Measures].[Doc Total TTC]"] ?? 0)
//                })
//                .GroupBy(x => x.Client)
//                .Select(g => new ClientDto
//                {
//                    Client = g.Key,
//                    TotalHT = g.Sum(x => x.TotalHT),
//                    TotalTTC = g.Sum(x => x.TotalTTC)
//                })
//                .OrderByDescending(x => x.TotalHT);
//        }

//        public static IEnumerable<ClientDto> ToClientDiscount(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data
//                .Where(d => d.ContainsKey("[Dim Customer].[Customer].[Customer].[MEMBER_CAPTION]"))
//                .Select(d => new ClientDto
//                {
//                    Client = d["[Dim Customer].[Customer].[Customer].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                    TotalDiscount = Convert.ToDecimal(d["[Measures].[Discount Amount]"] ?? 0),
//                    TotalHT = Convert.ToDecimal(d["[Measures].[Doc Total HT]"] ?? 0)
//                })
//                .GroupBy(x => x.Client)
//                .Select(g => new ClientDto
//                {
//                    Client = g.Key,
//                    TotalDiscount = g.Sum(x => x.TotalDiscount),
//                    DiscountRate = g.Sum(x => x.TotalHT) == 0 ? 0 : g.Sum(x => x.TotalDiscount) / g.Sum(x => x.TotalHT)
//                });
//        }

//        // ── Document / Type Mapping ───────────────────────────────────────────────────
//        public static IEnumerable<DocumentDto> ToDocumentDiscount(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data
//                .Where(d => d.ContainsKey("[Document Dim].[Document].[Document].[MEMBER_CAPTION]"))
//                .Select(d => new DocumentDto
//                {
//                    DocumentType = d["[Document Dim].[Document].[Document].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                    TotalValue = Convert.ToDecimal(d["[Measures].[Discount Amount]"] ?? 0)
//                })
//                .GroupBy(x => x.DocumentType)
//                .Select(g => new DocumentDto
//                {
//                    DocumentType = g.Key,
//                    TotalValue = g.Sum(x => x.TotalValue)
//                });
//        }

//        public static IEnumerable<DocumentDto> ToFodec(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data
//                .Where(d => d.ContainsKey("[Document Dim].[Document].[Document].[MEMBER_CAPTION]"))
//                .Select(d => new DocumentDto
//                {
//                    DocumentType = d["[Document Dim].[Document].[Document].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                    TotalValue = Convert.ToDecimal(d["[Measures].[FODEC Amount]"] ?? 0)
//                });
//        }

//        // ── Tax Mapping ───────────────────────────────────────────────────────────────
//        public static IEnumerable<DocumentDto> ToTvaByMonth(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data
//                .Select(d => new DocumentDto
//                {
//                    Month = d["[Dim Date].[Month Name].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                    TotalValue = Convert.ToDecimal(d["[Measures].[Doc Total TVA]"] ?? 0),
//                    Rate = Convert.ToDecimal(d["[Measures].[Doc Tx Tva]"] ?? 0)
//                })
//                .GroupBy(x => x.Month)
//                .Select(g => new DocumentDto
//                {
//                    Month = g.Key,
//                    TotalValue = g.Sum(x => x.TotalValue),
//                    Rate = g.Average(x => x.Rate)
//                })
//                .OrderBy(x => x.Month);
//        }

//        public static IEnumerable<DocumentDto> ToTaxQuarter(IEnumerable<Dictionary<string, object?>> data)
//        {
//            var result = new Dictionary<string, DocumentDto>();

//            foreach (var row in data)
//            {
//                foreach (var key in row.Keys.Where(k => k.Contains("[Dim Date].[Quarter].&[")))
//                {
//                    var raw = key.Split('[', ']')[2];
//                    if (raw.Length != 8) continue;

//                    var year = raw.Substring(0, 4);
//                    var month = int.Parse(raw.Substring(4, 2));
//                    var quarter = $"{year}-Q{Math.Ceiling(month / 3.0)}";

//                    var ttc = Convert.ToDecimal(row[key + "_TTC"] ?? 0);
//                    var tva = Convert.ToDecimal(row[key + "_TVA"] ?? 0);

//                    if (!result.ContainsKey(quarter))
//                        result[quarter] = new DocumentDto { Quarter = quarter };

//                    result[quarter].TotalValue += ttc;
//                    result[quarter].Rate += ttc == 0 ? 0 : tva / ttc;
//                }
//            }

//            return result.Values.OrderBy(x => x.Quarter);
//        }
//        public static IEnumerable<DocumentDto> ToRevenueByQuarterAndDocType(IEnumerable<Dictionary<string, object?>> data)
//        {
//            var result = new List<DocumentDto>();

//            foreach (var row in data)
//            {
//                var docType = row["[Document Dim].[Document].[Document].[MEMBER_CAPTION]"]?.ToString() ?? "";

//                foreach (var key in row.Keys.Where(k => k.Contains("[Dim Date].[Quarter].&[")))
//                {
//                    var raw = key.Split('[', ']')[2];
//                    if (raw.Length != 8) continue;

//                    var year = raw.Substring(0, 4);
//                    var month = int.Parse(raw.Substring(4, 2));
//                    var quarter = $"{year}-Q{Math.Ceiling(month / 3.0)}";

//                    var value = Convert.ToDecimal(row[key] ?? 0);

//                    result.Add(new DocumentDto
//                    {
//                        DocumentType = docType,
//                        Quarter = quarter,
//                        TotalValue = value
//                    });
//                }
//            }

//            return result;
//        }
//        public static IEnumerable<DocumentDto> ToFodecByDocType(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data
//                .Select(d => new DocumentDto
//                {
//                    DocumentType = d["[Document Dim].[Document].[Document].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                    TotalValue = Convert.ToDecimal(d["[Measures].[Doc Total Fodec]"] ?? 0),
//                    Rate = Convert.ToDecimal(d["[Measures].[Doc Val Fodec]"] ?? 0)
//                });
//        }
//        public static IEnumerable<PeriodDto> ToTaxBurdenQuarter(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data.Select(d => new PeriodDto
//            {
//                Label = d["[Dim Date].[Quarter].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                Value = Convert.ToDecimal(d["[Measures].[TVA Ratio %]"] ?? 0)
//            });
//        }
//        public static IEnumerable<ClientDto> ToYoYClient(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data.Select(d => new ClientDto
//            {
//                Client = d["[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                PreviousYear = Convert.ToDecimal(d.FirstOrDefault(x => x.Key.Contains("Revenue") && x.Key.Contains("202")).Value ?? 0),
//                CurrentYear = Convert.ToDecimal(d.FirstOrDefault(x => x.Key.Contains("Revenue") && !x.Key.Contains("202")).Value ?? 0),
//                Growth = Convert.ToDecimal(d["[Measures].[YoY Growth %]"] ?? 0)
//            });
//        }
//        public static IEnumerable<PeriodDto> ToYoYVolumeQuarter(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data.Select(d => new PeriodDto
//            {
//                Label = d["[Dim Date].[Quarter].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                PreviousValue = Convert.ToDecimal(d.FirstOrDefault(x => x.Key.Contains("Qty") && x.Key.Contains("202")).Value ?? 0),
//                CurrentValue = Convert.ToDecimal(d.FirstOrDefault(x => x.Key.Contains("Qty") && !x.Key.Contains("202")).Value ?? 0),
//                Growth = Convert.ToDecimal(d["[Measures].[Volume Growth %]"] ?? 0)
//            });
//        }
//        public static IEnumerable<PeriodDto> ToYtd(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data.Select(d => new PeriodDto
//            {
//                Label = d["[Dim Date].[Month Name].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                Value = Convert.ToDecimal(d["[Measures].[YTD Revenue HT]"] ?? 0)
//            });
//        }

//        public static IEnumerable<PeriodDto> ToQtd(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data.Select(d => new PeriodDto
//            {
//                Label = d["[Dim Date].[Month Name].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                Value = Convert.ToDecimal(d["[Measures].[QTD Revenue HT]"] ?? 0)
//            });
//        }
//        public static IEnumerable<ClientDto> ToMarginByClient(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data.Select(d => new ClientDto
//            {
//                Client = d["[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                TotalHT = Convert.ToDecimal(d["[Measures].[Doc Total HT]"] ?? 0),
//                Cost = Convert.ToDecimal(d["[Measures].[Doc P Revient]"] ?? 0),
//                Margin = Convert.ToDecimal(d["[Measures].[Gross Margin]"] ?? 0),
//                MarginRate = Convert.ToDecimal(d["[Measures].[Margin Rate %]"] ?? 0)
//            });
//        }
//        public static IEnumerable<DocumentDto> ToMonthlyMargin(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data.Select(d => new DocumentDto
//            {
//                Month = d["[Dim Date].[Month Name].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                TotalValue = Convert.ToDecimal(d["[Measures].[Doc Total HT]"] ?? 0),
//                Cost = Convert.ToDecimal(d["[Measures].[Doc P Revient]"] ?? 0),
//                Margin = Convert.ToDecimal(d["[Measures].[Gross Margin]"] ?? 0)
//            });
//        }
//        public static IEnumerable<ClientDto> ToClientBalance(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data.Select(d => new ClientDto
//            {
//                Client = d["[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                TotalTTC = Convert.ToDecimal(d["[Measures].[Doc Total TTC]"] ?? 0),
//                Paid = Convert.ToDecimal(d["[Measures].[Doc Acompte]"] ?? 0),
//                Remaining = Convert.ToDecimal(d["[Measures].[Doc Reste]"] ?? 0)
//            });
//        }
//        public static IEnumerable<ClientDto> ToRiskClients(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data.Select(d => new ClientDto
//            {
//                Client = d["[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                TotalHT = Convert.ToDecimal(d["[Measures].[Doc Total HT]"] ?? 0),
//                DiscountRate = Convert.ToDecimal(d["[Measures].[Doc Taux Rem]"] ?? 0),
//                Margin = Convert.ToDecimal(d["[Measures].[Gross Margin]"] ?? 0)
//            });
//        }
//        public static IEnumerable<ClientDto> ToAvgOrderValue(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data.Select(d => new ClientDto
//            {
//                Client = d["[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                TotalHT = Convert.ToDecimal(d["[Measures].[Doc Total HT]"] ?? 0),
//                OrderCount = Convert.ToInt32(d["[Measures].[Document Fact Nombre]"] ?? 0),
//                AvgOrderValue = Convert.ToDecimal(d["[Measures].[Avg Order Value]"] ?? 0)
//            });
//        }
//        public static IEnumerable<DocumentDto> ToVolume(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data.Select(d => new DocumentDto
//            {
//                DocumentType = d["[Document Dim].[Document].[Document].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                TypeAV = d["[Document Dim].[Type AV].[Type AV].[MEMBER_CAPTION]"]?.ToString(),
//                Quarter = d["[Dim Date].[Quarter].[MEMBER_CAPTION]"]?.ToString(),
//                Quantity = Convert.ToDecimal(d["[Measures].[Doc Qte]"] ?? 0)
//            });
//        }
//        public static IEnumerable<ClientDto> ToRevenueShare(IEnumerable<Dictionary<string, object?>> data)
//        {
//            return data.Select(d => new ClientDto
//            {
//                Client = d["[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]"]?.ToString() ?? "",
//                Revenue = Convert.ToDecimal(d["[Measures].[Doc Total HT]"] ?? 0),
//                SharePercent = Convert.ToDecimal(d["[Measures].[Revenue Share %]"] ?? 0)
//            });
//        }
//    }
//}
using System;
using System.Collections.Generic;
using System.Linq;

namespace MdxServices.Dto
{
    public static class MdxMapper
    {
        // ═══════════════════════════════════════════════════════════════════════
        // 🔧 HELPERS — clés MDX exactes issues de DocumentCubeQueries
        // ═══════════════════════════════════════════════════════════════════════

        // Dimensions
        private const string KeyYear = "[Dim Date].[Year].[Year].[MEMBER_CAPTION]";
        private const string KeyMonth = "[Dim Date].[Hiérarchie].[Month Name].[MEMBER_CAPTION]";
        private const string KeyClient = "[Tiers Dim].[Tiers].[Tiers].[MEMBER_CAPTION]";
        private const string KeyDocType = "[Document Dim].[Document].[Document].[MEMBER_CAPTION]";
        private const string KeyTypeAV = "[Document Dim].[Type AV].[Type AV].[MEMBER_CAPTION]";
        private const string KeyQuarterMember = "[Dim Date].[Quarter].[Quarter].[MEMBER_CAPTION]";

        // Measures — raw
        private const string KeyHT = "[Measures].[Doc Total HT]";
        private const string KeyTTC = "[Measures].[Doc Total TTC]";
        private const string KeyTVA = "[Measures].[Doc Total TVA]";
        private const string KeyTxTVA = "[Measures].[Doc Tx Tva]";
        private const string KeyRemise = "[Measures].[Doc Total Remise]";
        private const string KeyTauxRem = "[Measures].[Doc Taux Rem]";
        private const string KeyValRem = "[Measures].[Doc Val Rem]";
        private const string KeyFodec = "[Measures].[Doc Total Fodec]";
        private const string KeyValFodec = "[Measures].[Doc Val Fodec]";
        private const string KeyCost = "[Measures].[Doc P Revient]";
        private const string KeyAcompte = "[Measures].[Doc Acompte]";
        private const string KeyReste = "[Measures].[Doc Reste]";
        private const string KeyNbDoc = "[Measures].[Document Fact Nombre]";

        // Measures — computed by MDX WITH MEMBER
        private const string KeyDiscRatio = "[Measures].[Discount Ratio %]";
        private const string KeyGrossMargin = "[Measures].[Gross Margin]";
        private const string KeyMarginRate = "[Measures].[Margin Rate %]";
        private const string KeyAvgOrder = "[Measures].[Avg Order Value]";
        private const string KeyRevenueShare = "[Measures].[Revenue Share %]";
        private const string KeyTvaRatio = "[Measures].[TVA Ratio %]";
        private const string KeyVolGrowth = "[Measures].[Volume Growth %]";

        private static decimal Dec(Dictionary<string, object?> d, string key)
            => d.ContainsKey(key) ? Convert.ToDecimal(d[key] ?? 0) : 0m;

        private static string Str(Dictionary<string, object?> d, string key)
            => d.ContainsKey(key) ? d[key]?.ToString() ?? "" : "";

        private static string QuarterLabel(string raw8)
        {
            // "20231001" → "2023-Q4"
            var year = raw8.Substring(0, 4);
            var month = int.Parse(raw8.Substring(4, 2));
            return $"{year}-Q{(int)Math.Ceiling(month / 3.0)}";
        }

        // ═══════════════════════════════════════════════════════════════════════
        // 1. 📊 REVENUE
        // Queries: RevenueByYear · RevenueByMonth · RevenueByQuarterAndDocType
        //          RevenueByClientDesc · Top10ClientsByRevenue · Bottom10ClientsByRevenue
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// RevenueByYear → PeriodDto
        /// Label = year | Value = TotalHT | CurrentValue = TotalTTC
        /// </summary>
        public static IEnumerable<PeriodDto> ToYearRevenue(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyYear))
                .Select(d => new { Label = Str(d, KeyYear), HT = Dec(d, KeyHT), TTC = Dec(d, KeyTTC) })
                .Where(x => !string.IsNullOrEmpty(x.Label))
                .GroupBy(x => x.Label)
                .Select(g => new PeriodDto
                {
                    Label = g.Key,
                    Value = g.Sum(x => x.HT),
                    CurrentValue = g.Sum(x => x.TTC)
                })
                .OrderBy(x => x.Label);
        }

        /// <summary>
        /// RevenueByMonth → PeriodDto
        /// Label = month name | Value = TotalHT
        /// </summary>
        public static IEnumerable<PeriodDto> ToMonthRevenue(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyMonth))
                .GroupBy(d => Str(d, KeyMonth))
                .Select(g => new PeriodDto
                {
                    Label = g.Key,
                    Value = g.Sum(d => Dec(d, KeyHT))
                })
                .OrderBy(x => x.Label);
        }

        /// <summary>
        /// RevenueByQuarterAndDocType → PeriodDto per quarter
        /// AVOIR values are subtracted (credit notes).
        /// </summary>
        public static IEnumerable<PeriodDto> ToQuarterRevenue(
            IEnumerable<Dictionary<string, object?>> data)
        {
            var result = new Dictionary<string, decimal>();

            foreach (var row in data)
            {
                var docType = Str(row, KeyDocType);

                foreach (var key in row.Keys.Where(k => k.Contains("[Dim Date].[Quarter].&[")))
                {
                    var raw = key.Split('[', ']')[2];
                    if (raw.Length != 8) continue;

                    var quarter = QuarterLabel(raw);
                    var value = Convert.ToDecimal(row[key] ?? 0);

                    if (docType == "AVOIR") value = -value;

                    if (!result.ContainsKey(quarter)) result[quarter] = 0;
                    result[quarter] += value;
                }
            }

            return result
                .Select(kv => new PeriodDto { Label = kv.Key, Value = kv.Value })
                .OrderBy(x => x.Label);
        }

        /// <summary>
        /// RevenueByClientDesc / Top10 / Bottom10 → ClientDto
        /// MDX handles ordering (BDESC/TOPCOUNT/BOTTOMCOUNT) — no re-sort needed.
        /// </summary>
        public static IEnumerable<ClientDto> ToClientRevenue(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyClient))
                .GroupBy(d => Str(d, KeyClient))
                .Select(g => new ClientDto
                {
                    Client = g.Key,
                    TotalHT = g.Sum(d => Dec(d, KeyHT)),
                    TotalTTC = g.Sum(d => Dec(d, KeyTTC))
                });
        }

        // ═══════════════════════════════════════════════════════════════════════
        // 2. 💸 DISCOUNT
        // Queries: DiscountRatioByYear · Top10ClientsByDiscount · DiscountByDocumentType
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// DiscountRatioByYear → PeriodDto
        /// Label = year | Value = TotalHT | PreviousValue = TotalDiscount | Growth = DiscountRatio%
        /// </summary>
        public static IEnumerable<PeriodDto> ToDiscountRatioByYear(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyYear))
                .GroupBy(d => Str(d, KeyYear))
                .Select(g => new PeriodDto
                {
                    Label = g.Key,
                    Value = g.Sum(d => Dec(d, KeyHT)),
                    PreviousValue = g.Sum(d => Dec(d, KeyRemise)),
                    Growth = g.Average(d => Dec(d, KeyDiscRatio)) // computed by MDX
                })
                .OrderBy(x => x.Label);
        }

        /// <summary>
        /// Top10ClientsByDiscount → ClientDto
        /// TotalDiscount + DiscountRate (TauxRem from MDX)
        /// </summary>
        public static IEnumerable<ClientDto> ToClientDiscount(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyClient))
                .GroupBy(d => Str(d, KeyClient))
                .Select(g => new ClientDto
                {
                    Client = g.Key,
                    TotalDiscount = g.Sum(d => Dec(d, KeyRemise)),
                    DiscountRate = g.Average(d => Dec(d, KeyTauxRem))
                });
        }

        /// <summary>
        /// DiscountByDocumentType → DocumentDto
        /// TotalValue = TotalRemise | Rate = ValRem
        /// </summary>
        public static IEnumerable<DocumentDto> ToDocumentDiscount(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyDocType))
                .GroupBy(d => Str(d, KeyDocType))
                .Select(g => new DocumentDto
                {
                    DocumentType = g.Key,
                    TotalValue = g.Sum(d => Dec(d, KeyRemise)),
                    Rate = g.Sum(d => Dec(d, KeyValRem))
                })
                .OrderByDescending(x => x.TotalValue);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // 3. 🧾 TAX
        // Queries: TvaByMonth · FodecByDocumentType · TaxBurdenRatioByQuarter
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// TvaByMonth → DocumentDto
        /// Month | TotalValue = TotalTVA | Rate = TxTva
        /// </summary>
        public static IEnumerable<DocumentDto> ToTvaMonth(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyMonth))
                .GroupBy(d => Str(d, KeyMonth))
                .Select(g => new DocumentDto
                {
                    Month = g.Key,
                    TotalValue = g.Sum(d => Dec(d, KeyTVA)),
                    Rate = g.Average(d => Dec(d, KeyTxTVA))
                })
                .OrderBy(x => x.Month);
        }

        /// <summary>
        /// FodecByDocumentType → DocumentDto
        /// DocumentType | TotalValue = TotalFodec | Rate = ValFodec
        /// </summary>
        public static IEnumerable<DocumentDto> ToFodec(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyDocType))
                .GroupBy(d => Str(d, KeyDocType))
                .Select(g => new DocumentDto
                {
                    DocumentType = g.Key,
                    TotalValue = g.Sum(d => Dec(d, KeyFodec)),
                    Rate = g.Sum(d => Dec(d, KeyValFodec))
                })
                .OrderByDescending(x => x.TotalValue);
        }

        /// <summary>
        /// TaxBurdenRatioByQuarter → DocumentDto
        /// Quarter on ROWS | TotalValue = TotalTTC | Cost = TotalTVA | Rate = TvaRatio% (MDX)
        /// </summary>
        public static IEnumerable<DocumentDto> ToTaxQuarter(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyQuarterMember))
                .GroupBy(d => Str(d, KeyQuarterMember))
                .Select(g => new DocumentDto
                {
                    Quarter = g.Key,
                    TotalValue = g.Sum(d => Dec(d, KeyTTC)),
                    Cost = g.Sum(d => Dec(d, KeyTVA)),
                    Rate = g.Average(d => Dec(d, KeyTvaRatio))
                })
                .OrderBy(x => x.Quarter);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // 4. 📈 YOY
        // Queries: YoYRevenueByClient · YoYVolumeByQuarter
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// YoYRevenueByClient → ClientDto
        /// Pass the same years used when calling YoYRevenueByClient(currentYear, priorYear).
        /// MDX named members: [Measures].[Revenue {year}] and [Measures].[YoY Growth %]
        /// </summary>
        public static IEnumerable<ClientDto> ToYoYRevenueByClient(
            IEnumerable<Dictionary<string, object?>> data,
            int currentYear,
            int priorYear)
        {
            var keyPrev = $"[Measures].[Revenue {priorYear}]";
            var keyCurr = $"[Measures].[Revenue {currentYear}]";
            const string keyGrowth = "[Measures].[YoY Growth %]";

            return data
                .Where(d => d.ContainsKey(KeyClient))
                .GroupBy(d => Str(d, KeyClient))
                .Select(g => new ClientDto
                {
                    Client = g.Key,
                    PreviousYear = g.Sum(d => Dec(d, keyPrev)),
                    CurrentYear = g.Sum(d => Dec(d, keyCurr)),
                    Growth = g.Average(d => Dec(d, keyGrowth))
                })
                .OrderByDescending(x => x.CurrentYear);
        }

        public static IEnumerable<PeriodDto> ToYoYVolumeByQuarter(
            IEnumerable<Dictionary<string, object?>> data,
            int currentYear,
            int priorYear)
        {
            var keyPrev = $"[Measures].[Qty {priorYear}]";
            var keyCurr = $"[Measures].[Qty {currentYear}]";

            return data
                .Where(d => d.ContainsKey(KeyQuarterMember))
                .GroupBy(d => Str(d, KeyQuarterMember))
                .Select(g => new PeriodDto
                {
                    Label = g.Key,
                    PreviousValue = g.Sum(d => Dec(d, keyPrev)),
                    CurrentValue = g.Sum(d => Dec(d, keyCurr)),
                    Growth = g.Average(d => Dec(d, KeyVolGrowth))
                })
                .OrderBy(x => x.Label);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // 5. 📅 YTD / QTD
        // Queries: YtdRevenueByMonth · QtdRevenueByMonth
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// YtdRevenueByMonth → PeriodDto
        /// Label = month name | Value = cumulative YTD HT (computed by MDX SUM/YTD)
        /// </summary>
        public static IEnumerable<PeriodDto> ToYtdRevenue(
            IEnumerable<Dictionary<string, object?>> data)
        {
            const string keyYtd = "[Measures].[YTD Revenue HT]";

            return data
                .Where(d => d.ContainsKey(KeyMonth))
                .GroupBy(d => Str(d, KeyMonth))
                .Select(g => new PeriodDto
                {
                    Label = g.Key,
                    Value = g.Sum(d => Dec(d, keyYtd))
                })
                .OrderBy(x => x.Label);
        }

        /// <summary>
        /// QtdRevenueByMonth → PeriodDto
        /// Label = month name | Value = cumulative QTD HT (computed by MDX PERIODSTODATE)
        /// </summary>
        public static IEnumerable<PeriodDto> ToQtdRevenue(
            IEnumerable<Dictionary<string, object?>> data)
        {
            const string keyQtd = "[Measures].[QTD Revenue HT]";

            return data
                .Where(d => d.ContainsKey(KeyMonth))
                .GroupBy(d => Str(d, KeyMonth))
                .Select(g => new PeriodDto
                {
                    Label = g.Key,
                    Value = g.Sum(d => Dec(d, keyQtd))
                })
                .OrderBy(x => x.Label);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // 6. 💰 MARGIN
        // Queries: GrossMarginByClient · MonthlyMarginTrend
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// GrossMarginByClient → ClientDto
        /// MDX computes [Gross Margin] and [Margin Rate %] as named members.
        /// </summary>
        public static IEnumerable<ClientDto> ToGrossMarginByClient(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyClient))
                .GroupBy(d => Str(d, KeyClient))
                .Select(g => new ClientDto
                {
                    Client = g.Key,
                    TotalHT = g.Sum(d => Dec(d, KeyHT)),
                    Cost = g.Sum(d => Dec(d, KeyCost)),
                    Margin = g.Sum(d => Dec(d, KeyGrossMargin)),
                    MarginRate = g.Average(d => Dec(d, KeyMarginRate))
                })
                .OrderByDescending(x => x.Margin);
        }

        /// <summary>
        /// MonthlyMarginTrend → DocumentDto
        /// Month | TotalValue = TotalHT | Cost = P.Revient | Margin = GrossMargin (MDX)
        /// </summary>
        public static IEnumerable<DocumentDto> ToMonthlyMargin(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyMonth))
                .GroupBy(d => Str(d, KeyMonth))
                .Select(g => new DocumentDto
                {
                    Month = g.Key,
                    TotalValue = g.Sum(d => Dec(d, KeyHT)),
                    Cost = g.Sum(d => Dec(d, KeyCost)),
                    Margin = g.Sum(d => Dec(d, KeyGrossMargin))
                })
                .OrderBy(x => x.Month);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // 7. 👥 CLIENT SEGMENTATION
        // Queries: ClientsAboveAverageRevenue · ClientsWithOutstandingBalance
        //          HighDiscountLowMarginClients
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// ClientsAboveAverageRevenue → ClientDto
        /// MDX FILTER already removes below-average clients — just map.
        /// </summary>
        public static IEnumerable<ClientDto> ToClientsAboveAverage(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyClient))
                .GroupBy(d => Str(d, KeyClient))
                .Select(g => new ClientDto
                {
                    Client = g.Key,
                    TotalHT = g.Sum(d => Dec(d, KeyHT)),
                    TotalTTC = g.Sum(d => Dec(d, KeyTTC))
                })
                .OrderByDescending(x => x.TotalHT);
        }

        /// <summary>
        /// ClientsWithOutstandingBalance → ClientDto
        /// TotalTTC | Paid = Doc Acompte | Remaining = Doc Reste
        /// MDX ORDER(NONEMPTY(..., [Doc Reste])) handles sorting.
        /// </summary>
        public static IEnumerable<ClientDto> ToClientBalance(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyClient))
                .GroupBy(d => Str(d, KeyClient))
                .Select(g => new ClientDto
                {
                    Client = g.Key,
                    TotalTTC = g.Sum(d => Dec(d, KeyTTC)),
                    Paid = g.Sum(d => Dec(d, KeyAcompte)),
                    Remaining = g.Sum(d => Dec(d, KeyReste))
                })
                .Where(x => x.Remaining > 0);
        }

        /// <summary>
        /// HighDiscountLowMarginClients → ClientDto
        /// MDX FILTER (TauxRem > 10 AND GrossMargin < 0) already restricts rows.
        /// DiscountRate = Doc Taux Rem | Margin = Gross Margin (MDX named member)
        /// </summary>
        public static IEnumerable<ClientDto> ToRiskClients(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyClient))
                .GroupBy(d => Str(d, KeyClient))
                .Select(g => new ClientDto
                {
                    Client = g.Key,
                    TotalHT = g.Sum(d => Dec(d, KeyHT)),
                    DiscountRate = g.Average(d => Dec(d, KeyTauxRem)),
                    Margin = g.Sum(d => Dec(d, KeyGrossMargin))
                })
                .OrderBy(x => x.Margin);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // 8. 📦 DOCUMENTS / VOLUME
        // Queries: AverageOrderValueByClient · VolumeByDocTypeAndQuarter
        //          RevenueConcentrationTop20Pct
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// AverageOrderValueByClient → ClientDto
        /// MDX computes [Avg Order Value] — use it directly.
        /// OrderCount = Document Fact Nombre | AvgOrderValue = Avg Order Value (MDX)
        /// </summary>
        public static IEnumerable<ClientDto> ToAvgOrderByClient(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyClient))
                .GroupBy(d => Str(d, KeyClient))
                .Select(g => new ClientDto
                {
                    Client = g.Key,
                    TotalHT = g.Sum(d => Dec(d, KeyHT)),
                    OrderCount = (int)g.Sum(d => Dec(d, KeyNbDoc)),
                    AvgOrderValue = g.Average(d => Dec(d, KeyAvgOrder))
                })
                .OrderByDescending(x => x.AvgOrderValue);
        }

        /// <summary>
        /// VolumeByDocTypeAndQuarter → DocumentDto
        /// CROSSJOIN(Document, Type AV) on ROWS × Quarter keys on COLUMNS.
        /// </summary>
        public static IEnumerable<DocumentDto> ToVolumeByDocTypeAndQuarter(
            IEnumerable<Dictionary<string, object?>> data)
        {
            var result = new List<DocumentDto>();

            foreach (var row in data)
            {
                var docType = Str(row, KeyDocType);
                var typeAV = Str(row, KeyTypeAV);

                foreach (var key in row.Keys.Where(k => k.Contains("[Dim Date].[Quarter].&[")))
                {
                    var raw = key.Split('[', ']')[2];
                    if (raw.Length != 8) continue;

                    var quarter = QuarterLabel(raw);
                    var qty = Convert.ToDecimal(row[key] ?? 0);

                    var existing = result.FirstOrDefault(
                        v => v.DocumentType == docType && v.TypeAV == typeAV && v.Quarter == quarter);

                    if (existing != null)
                        existing.Quantity += qty;
                    else
                        result.Add(new DocumentDto
                        {
                            DocumentType = docType,
                            TypeAV = typeAV,
                            Quarter = quarter,
                            Quantity = qty
                        });
                }
            }

            return result.OrderBy(x => x.Quarter).ThenBy(x => x.DocumentType);
        }

        /// <summary>
        /// RevenueConcentrationTop20Pct → ClientDto
        /// MDX computes [Revenue Share %] — use it directly.
        /// Revenue stored in ClientDto.Revenue | SharePercent = Revenue Share % (MDX)
        /// </summary>
        public static IEnumerable<ClientDto> ToRevenueConcentration(
            IEnumerable<Dictionary<string, object?>> data)
        {
            return data
                .Where(d => d.ContainsKey(KeyClient))
                .GroupBy(d => Str(d, KeyClient))
                .Select(g => new ClientDto
                {
                    Client = g.Key,
                    Revenue = g.Sum(d => Dec(d, KeyHT)),
                    SharePercent = g.Average(d => Dec(d, KeyRevenueShare))
                })
                .OrderByDescending(x => x.Revenue);
        }
    }
}