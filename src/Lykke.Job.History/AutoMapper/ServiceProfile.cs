using System;
using System.Collections.Generic;
using AutoMapper;
using Lykke.Service.History.Contracts.Cqrs.Commands;
using Lykke.Service.History.Contracts.History;
using Lykke.Service.History.Core;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Models;
using TradeModel = Lykke.Service.PostProcessing.Contracts.Cqrs.Models.TradeModel;

namespace Lykke.Job.History.AutoMapper
{
    public class ServiceProfile : Profile
    {
        public ServiceProfile()
        {
            CreateMap<CashInProcessedEvent, Cashin>()
                .ForMember(x => x.Id, o => o.MapFrom(s => s.OperationId))
                .ForMember(x => x.State, o => o.UseValue(HistoryState.Finished))
                .ForMember(x => x.Volume, o => o.MapFrom(s => Math.Abs(s.Volume)));

            CreateMap<CashOutProcessedEvent, Cashout>()
                .ForMember(x => x.Id, o => o.MapFrom(s => s.OperationId))
                .ForMember(x => x.State, o => o.UseValue(HistoryState.Finished))
                .ForMember(x => x.Volume, o => o.MapFrom(s => -Math.Abs(s.Volume)));

            CreateMap<CashTransferProcessedEvent, IEnumerable<BaseHistoryRecord>>().ConvertUsing<CashTransferConverter>();

            CreateMap<TradeModel, Trade>();

            CreateMap<OrderModel, Order>();

            CreateMap<ExecutionProcessedEvent, IEnumerable<Order>>().ConvertUsing<ExecutionConverter>();

            CreateMap<OrderPlacedEvent, OrderEvent>()
                .ForMember(x => x.Status, o => o.UseValue(OrderStatus.Placed))
                .ForMember(x => x.Id, o => o.MapFrom(s => Utils.IncrementGuid(s.OrderId, (int)OrderStatus.Placed)))
                .ForMember(x => x.Timestamp, o => o.MapFrom(s => s.CreateDt));

            CreateMap<OrderCancelledEvent, OrderEvent>()
                .ForMember(x => x.Status, o => o.UseValue(OrderStatus.Cancelled))
                .ForMember(x => x.Id, o => o.MapFrom(s => Utils.IncrementGuid(s.OrderId, (int)OrderStatus.Cancelled)));

            CreateMap<CreateForwardCashinCommand, Cashin>()
                .ForMember(x => x.Id, o => o.MapFrom(s => s.OperationId))
                .ForMember(x => x.State, o => o.UseValue(HistoryState.Finished))
                .ForMember(x => x.Volume, o => o.MapFrom(s => Math.Abs(s.Volume)));

            CreateMap<BaseHistoryRecord, BaseHistoryModel>();

            CreateMap<Cashin, CashinModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();
            CreateMap<Cashout, CashoutModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();
            CreateMap<Trade, Service.History.Contracts.History.TradeModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();
            CreateMap<OrderEvent, OrderEventModel>()
                .IncludeBase<BaseHistoryRecord, BaseHistoryModel>();

            CreateMap<Order, OrderModel>();
        }
    }
}
