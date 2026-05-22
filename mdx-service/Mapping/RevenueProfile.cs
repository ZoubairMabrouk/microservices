//using AutoMapper;
//using MdxServices.Dto;
//using MdxServices.Models;

//namespace MdxServices.Mapping
//{
//    public class RevenueProfile : Profile
//    {
//        public RevenueProfile()
//        {
//            // ── Year Revenue ──────────────────────────────────────────────
//            CreateMap<YearRevenueDto>()
//                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Date.Year))
//                .ForMember(dest => dest.TotalHT, opt => opt.MapFrom(src => src.MontantHT))
//                .ForMember(dest => dest.TotalTTC, opt => opt.MapFrom(src => src.MontantTTC));

//            // ── Month Revenue ─────────────────────────────────────────────
//            CreateMap<Invoice, MonthRevenueDto>()
//                .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.Date.Month))
//                .ForMember(dest => dest.TotalHT, opt => opt.MapFrom(src => src.MontantHT));

//            // ── Quarter Revenue ───────────────────────────────────────────
//            // Populated via projection/query — no direct entity mapping needed.
//            // Use CreateMap<QuarterRevenueRaw, QuarterRevenueDto>() if you have an intermediate type.
//            CreateMap<QuarterRevenueRaw, QuarterRevenueDto>()
//                .ForMember(dest => dest.Quarter, opt => opt.MapFrom(src => src.Quarter))
//                .ForMember(dest => dest.Invoice, opt => opt.MapFrom(src => src.InvoiceAmount))
//                .ForMember(dest => dest.CreditNote, opt => opt.MapFrom(src => src.CreditNoteAmount))
//                .ForMember(dest => dest.DebitNote, opt => opt.MapFrom(src => src.DebitNoteAmount));

//            // ── Client Revenue ────────────────────────────────────────────
//            CreateMap<Invoice, ClientRevenueDto>()
//                .ForMember(dest => dest.Client, opt => opt.MapFrom(src => src.Client.Name))
//                .ForMember(dest => dest.TotalHT, opt => opt.MapFrom(src => src.MontantHT))
//                .ForMember(dest => dest.TotalTTC, opt => opt.MapFrom(src => src.MontantTTC));
//        }
//    }
//}