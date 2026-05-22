namespace MdxServices.Dto
{
    // DTO générique pour les clients
    public class ClientDto
    {
        public string Client { get; set; } = "";
        public decimal TotalHT { get; set; }
        public decimal TotalTTC { get; set; }
        public decimal Paid { get; set; }           // pour ClientBalanceDto
        public decimal Remaining { get; set; }      // pour ClientBalanceDto
        public decimal TotalDiscount { get; set; }  // pour ClientDiscountDto
        public decimal DiscountRate { get; set; }   // pour ClientDiscountDto / RiskClientDto
        public decimal Cost { get; set; }           // pour ClientMarginDto
        public decimal Margin { get; set; }         // pour ClientMarginDto / RiskClientDto
        public decimal MarginRate { get; set; }     // pour ClientMarginDto
        public int OrderCount { get; set; }         // pour AvgOrderDto
        public decimal AvgOrderValue { get; set; }  // pour AvgOrderDto
        public decimal Revenue { get; set; }        // pour RevenueShareDto
        public decimal SharePercent { get; set; }   // pour RevenueShareDto
        public decimal PreviousYear { get; set; }   // pour YoYClientDto
        public decimal CurrentYear { get; set; }    // pour YoYClientDto
        public decimal Growth { get; set; }         // pour YoYClientDto
    }

    // DTO pour les documents et types de taxe
    public class DocumentDto
    {
        public string DocumentType { get; set; } = "";
        public string TypeAV { get; set; }          // pour VolumeDto
        public string Quarter { get; set; }         // pour VolumeDto / TaxQuarterDto / YoYQuarterDto
        public string Month { get; set; }           // pour PeriodRevenueDto / TvaMonthDto / MonthlyMarginDto
        public decimal TotalValue { get; set; }     // TotalTTC, TotalHT, TotalDiscount, TotalFodec, etc.
        public decimal Rate { get; set; }           // TVA rate, DiscountRate, MarginRate
        public decimal Cost { get; set; }           // pour MonthlyMarginDto
        public decimal Margin { get; set; }         // pour MonthlyMarginDto
        public decimal Quantity { get; set; }       // pour VolumeDto
    }

    // DTO pour les périodes
    public class PeriodDto
    {
        public string Label { get; set; } = "";     // Month, Quarter, Year...
        public decimal Value { get; set; }          // TotalHT, TotalTTC, Revenue, etc.
        public decimal PreviousValue { get; set; }  // pour YoY
        public decimal CurrentValue { get; set; }   // pour YoY
        public decimal Growth { get; set; }         // pour YoY
    }
    public class MdxRequestDto
    {
        public string GeneratedQuery { get; set; }
    }
}