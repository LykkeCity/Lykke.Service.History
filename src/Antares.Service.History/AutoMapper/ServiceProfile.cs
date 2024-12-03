using Antares.Service.History.Contracts.History;
using Antares.Service.History.Contracts.Orders;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.Core.Domain.Orders;
using AutoMapper;

namespace Antares.Service.History.AutoMapper
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<Contracts.History.TradeModel, Trade>();

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
