using AutoMapper;
using Lykke.Service.History.Contracts.History;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Models;
using TradeModel = Lykke.Service.PostProcessing.Contracts.Cqrs.Models.TradeModel;

namespace Lykke.Service.History.AutoMapper
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<TradeModel, Trade>();

            CreateMap<OrderModel, Order>();

            CreateMap<BaseHistoryRecord, BaseHistoryModel>();

            CreateMap<Cashin, CashinModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();
            CreateMap<Cashout, CashoutModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();
            CreateMap<Trade, Contracts.History.TradeModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();
            CreateMap<OrderEvent, OrderEventModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();

            CreateMap<Order, OrderModel>();
        }
    }
}
