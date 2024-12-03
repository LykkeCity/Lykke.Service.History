using Antares.Service.History.Core.Domain.Enums;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.Core.Domain.Orders;
using Antares.Service.History.PostgresRepositories.Entities;
using AutoMapper;

namespace Antares.Service.History.PostgresRepositories.Mappings
{
    public class RepositoryProfile : Profile
    {
        public RepositoryProfile()
        {
            CreateMap<HistoryEntity, Cashin>()
                .ForMember(x => x.BlockchainHash, o => o.MapFrom(p => p.ContextObject.BlockchainHash))
                .ForMember(x => x.State, o => o.MapFrom(p => p.ContextObject.State))
                .ForMember(x => x.FeeSize, o => o.MapFrom(p => p.ContextObject.FeeSize))
                .ReverseMap()
                .ForPath(x => x.ContextObject.BlockchainHash, o => o.MapFrom(p => p.BlockchainHash))
                .ForPath(x => x.ContextObject.State, o => o.MapFrom(p => p.State))
                .ForPath(x => x.ContextObject.FeeSize, o => o.MapFrom(p => p.FeeSize))
                .ForMember(x => x.Type, o => o.UseValue(HistoryType.CashIn));

            CreateMap<HistoryEntity, Cashout>()
                .ForMember(x => x.BlockchainHash, o => o.MapFrom(p => p.ContextObject.BlockchainHash))
                .ForMember(x => x.State, o => o.MapFrom(p => p.ContextObject.State))
                .ForMember(x => x.FeeSize, o => o.MapFrom(p => p.ContextObject.FeeSize))
                .ReverseMap()
                .ForPath(x => x.ContextObject.BlockchainHash, o => o.MapFrom(p => p.BlockchainHash))
                .ForPath(x => x.ContextObject.State, o => o.MapFrom(p => p.State))
                .ForPath(x => x.ContextObject.FeeSize, o => o.MapFrom(p => p.FeeSize))
                .ForMember(x => x.Type, o => o.UseValue(HistoryType.CashOut));

            CreateMap<HistoryEntity, Trade>()
                .ForMember(x => x.BaseAssetId, o => o.MapFrom(p => p.AssetId))
                .ForMember(x => x.BaseVolume, o => o.MapFrom(p => p.Volume))
                .ForMember(x => x.Price, o => o.MapFrom(p => p.ContextObject.Price))
                .ForMember(x => x.FeeSize, o => o.MapFrom(p => p.ContextObject.FeeSize))
                .ForMember(x => x.FeeAssetId, o => o.MapFrom(p => p.ContextObject.FeeAssetId))
                .ForMember(x => x.Index, o => o.MapFrom(p => p.ContextObject.TradeIndex))
                .ForMember(x => x.Role, o => o.MapFrom(p => p.ContextObject.TradeRole))
                .ForMember(x => x.QuotingAssetId, o => o.MapFrom(p => p.ContextObject.TradeQuotingAssetId))
                .ForMember(x => x.QuotingVolume, o => o.MapFrom(p => p.ContextObject.TradeQuotingVolume))
                .ForMember(x => x.OrderId, o => o.MapFrom(p => p.ContextObject.OrderId))
                .ReverseMap()
                .ForMember(x => x.AssetId, o => o.MapFrom(p => p.BaseAssetId))
                .ForMember(x => x.Volume, o => o.MapFrom(p => p.BaseVolume))
                .ForPath(x => x.ContextObject.Price, o => o.MapFrom(p => p.Price))
                .ForPath(x => x.ContextObject.FeeSize, o => o.MapFrom(p => p.FeeSize))
                .ForPath(x => x.ContextObject.FeeAssetId, o => o.MapFrom(p => p.FeeAssetId))
                .ForPath(x => x.ContextObject.TradeIndex, o => o.MapFrom(p => p.Index))
                .ForPath(x => x.ContextObject.TradeRole, o => o.MapFrom(p => p.Role))
                .ForPath(x => x.ContextObject.TradeQuotingAssetId, o => o.MapFrom(p => p.QuotingAssetId))
                .ForPath(x => x.ContextObject.TradeQuotingVolume, o => o.MapFrom(p => p.QuotingVolume))
                .ForPath(x => x.ContextObject.OrderId, o => o.MapFrom(p => p.OrderId))
                .ForMember(x => x.Type, o => o.UseValue(HistoryType.Trade));

            CreateMap<HistoryEntity, OrderEvent>()
                .ForMember(x => x.Price, o => o.MapFrom(p => p.ContextObject.Price))
                .ForMember(x => x.OrderId, o => o.MapFrom(p => p.ContextObject.OrderId))
                .ForMember(x => x.Status, o => o.MapFrom(p => p.ContextObject.OrderEventStatus))
                .ReverseMap()
                .ForPath(x => x.ContextObject.Price, o => o.MapFrom(p => p.Price))
                .ForPath(x => x.ContextObject.OrderId, o => o.MapFrom(p => p.OrderId))
                .ForPath(x => x.ContextObject.OrderEventStatus, o => o.MapFrom(p => p.Status))
                .ForMember(x => x.Type, o => o.UseValue(HistoryType.OrderEvent));

            CreateMap<OrderEntity, Order>();
        }
    }
}
