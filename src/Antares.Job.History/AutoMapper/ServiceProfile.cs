using System;
using Antares.Service.History.Contracts.Cqrs.Commands;
using Antares.Service.History.Contracts.History;
using Antares.Service.History.Core.Domain.Enums;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.Core.Domain.Orders;
using AutoMapper;
using OrderModel = Antares.Job.History.RabbitSubscribers.Models.OrderModel;
using TradeModel = Antares.Job.History.RabbitSubscribers.Models.TradeModel;

namespace Antares.Job.History.AutoMapper
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<TradeModel, Trade>();

            CreateMap<OrderModel, Order>();

            CreateMap<CreateForwardCashinCommand, Cashin>()
                .ForMember(x => x.Id, o => o.MapFrom(s => s.OperationId))
                .ForMember(x => x.State, o => o.UseValue(HistoryState.Finished))
                .ForMember(x => x.Volume, o => o.MapFrom(s => Math.Abs(s.Volume)));

            CreateMap<BaseHistoryRecord, BaseHistoryModel>();

            CreateMap<Cashin, CashinModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();
            CreateMap<Cashout, CashoutModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();
            CreateMap<Trade, Antares.Service.History.Contracts.History.TradeModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();
            CreateMap<OrderEvent, OrderEventModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();

            CreateMap<Order, OrderModel>();
        }
    }
}
