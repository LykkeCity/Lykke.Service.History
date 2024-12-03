using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.Core.Domain.Orders;
using Antares.Service.History.GrpcContract.Common;
using Antares.Service.History.GrpcContract.Orders;
using Google.Protobuf.WellKnownTypes;
using HistoryType = Antares.Service.History.Core.Domain.Enums.HistoryType;
using OrderSide = Antares.Service.History.Core.Domain.Enums.OrderSide;
using OrderStatus = Antares.Service.History.Core.Domain.Enums.OrderStatus;
using OrderType = Antares.Service.History.Core.Domain.Enums.OrderType;
using TradeRole = Antares.Service.History.Core.Domain.Enums.TradeRole;

namespace Antares.Service.History.GrpcServices.Mappers
{
    public static class GrpcMapper
    {
        public static IReadOnlyCollection<HistoryResponseItem> Map(IEnumerable<BaseHistoryRecord> records)
        {
            return records.Select(Map).ToArray();
        }

        public static HistoryResponseItem Map(BaseHistoryRecord baseHistoryRecord)
        {
            var item = new HistoryResponseItem()
            {
                Id = baseHistoryRecord.Id.ToString(),
                Timestamp = baseHistoryRecord.Timestamp.ToUniversalTime().ToTimestamp(),
                WalletId = baseHistoryRecord.WalletId.ToString(),
            };

            switch (baseHistoryRecord.Type)
            {
                case HistoryType.CashIn:
                    {
                        var cashinModel = (Antares.Service.History.Core.Domain.History.Cashin)baseHistoryRecord;
                        item.Type = GrpcContract.Common.HistoryType.CashIn;
                        item.CashIn = new CashInModel()
                        {
                            AssetId = cashinModel.AssetId,
                            BlockchainHash = cashinModel.BlockchainHash,
                            FeeSize = cashinModel.FeeSize,
                            Volume = cashinModel.Volume
                        };
                        break;
                    }
                case HistoryType.CashOut:
                    {
                        var cashout = (Antares.Service.History.Core.Domain.History.Cashout)baseHistoryRecord;
                        item.Type = GrpcContract.Common.HistoryType.CashOut;
                        item.CashOut = new CashOutModel()
                        {
                            AssetId = cashout.AssetId,
                            BlockchainHash = cashout.BlockchainHash,
                            FeeSize = cashout.FeeSize,
                            Volume = cashout.Volume
                        };
                        break;
                    }
                case HistoryType.Trade:
                    {
                        var trade = (Antares.Service.History.Core.Domain.History.Trade)baseHistoryRecord;
                        item.Type = GrpcContract.Common.HistoryType.Trade;
                        item.Trade = new TradeModel()
                        {
                            FeeSize = trade.FeeSize,
                            AssetPairId = trade.AssetPairId,
                            OrderId = trade.OrderId.ToString(),
                            BaseAssetId = trade.BaseAssetId,
                            BaseVolume = trade.BaseVolume,
                            FeeAssetId = trade.FeeAssetId,
                            Index = trade.Index,
                            Price = trade.Price,
                            QuotingAssetId = trade.QuotingAssetId,
                            QuotingVolume = trade.QuotingVolume,
                            Role = trade.Role switch
                            {
                                TradeRole.Unknown => GrpcContract.Common.TradeRole.Unknown,
                                TradeRole.Maker => GrpcContract.Common.TradeRole.Maker,
                                TradeRole.Taker => GrpcContract.Common.TradeRole.Taker,
                                _ => throw new ArgumentOutOfRangeException(nameof(trade.Role), trade.Role, null)
                            }
                        };
                        break;
                    }
                case HistoryType.OrderEvent:
                    {
                        var orderEvent = (Antares.Service.History.Core.Domain.History.OrderEvent)baseHistoryRecord;
                        item.Type = GrpcContract.Common.HistoryType.OrderEvent;
                        item.OrderEvent = new OrderEventModel()
                        {
                            AssetPairId = orderEvent.AssetPairId,
                            OrderId = orderEvent.OrderId.ToString(),
                            Price = orderEvent.Price,
                            Status = orderEvent.Status switch
                            {
                                OrderStatus.Unknown => GrpcContract.Common.OrderStatus.UnknownOrder,
                                OrderStatus.Placed => GrpcContract.Common.OrderStatus.Placed,
                                OrderStatus.PartiallyMatched => GrpcContract.Common.OrderStatus.PartiallyMatched,
                                OrderStatus.Matched => GrpcContract.Common.OrderStatus.Matched,
                                OrderStatus.Pending => GrpcContract.Common.OrderStatus.Pending,
                                OrderStatus.Cancelled => GrpcContract.Common.OrderStatus.Cancelled,
                                OrderStatus.Replaced => GrpcContract.Common.OrderStatus.Replaced,
                                OrderStatus.Rejected => GrpcContract.Common.OrderStatus.Rejected,
                                _ => throw new ArgumentOutOfRangeException(nameof(orderEvent.Status), orderEvent.Status, null)
                            },
                            Volume = orderEvent.Volume
                        };
                        break;

                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(baseHistoryRecord.Type), baseHistoryRecord.Type, null);
            }

            return item;
        }

        public static OrderModel MapOrder(Order order)
        {
            return new OrderModel()
            {
                Volume = order.Volume,
                AssetPairId = order.AssetPairId,
                CreateDt = order.CreateDt.ToUniversalTime().ToTimestamp(),
                Id = order.Id.ToString(),
                Type = order.Type switch
                {
                    OrderType.Unknown => GrpcContract.Orders.OrderType.UnknownType,
                    OrderType.Market => GrpcContract.Orders.OrderType.Market,
                    OrderType.Limit => GrpcContract.Orders.OrderType.Limit,
                    OrderType.StopLimit => GrpcContract.Orders.OrderType.StopLimit,
                    _ => throw new ArgumentOutOfRangeException()
                },
                WalletId = order.WalletId.ToString(),
                Status = order.Status switch
                {
                    OrderStatus.Unknown => GrpcContract.Common.OrderStatus.UnknownOrder,
                    OrderStatus.Placed => GrpcContract.Common.OrderStatus.Placed,
                    OrderStatus.PartiallyMatched => GrpcContract.Common.OrderStatus.PartiallyMatched,
                    OrderStatus.Matched => GrpcContract.Common.OrderStatus.Matched,
                    OrderStatus.Pending => GrpcContract.Common.OrderStatus.Pending,
                    OrderStatus.Cancelled => GrpcContract.Common.OrderStatus.Cancelled,
                    OrderStatus.Replaced => GrpcContract.Common.OrderStatus.Replaced,
                    OrderStatus.Rejected => GrpcContract.Common.OrderStatus.Rejected,
                    _ => throw new ArgumentOutOfRangeException()
                },
                Price = order.Price,
                LowerLimitPrice = order.LowerLimitPrice,
                LowerPrice = order.LowerPrice,
                MatchDt = order.MatchDt?.ToUniversalTime().ToTimestamp(),
                MatchingId = order.MatchingId.ToString(),
                RegisterDt = order.RegisterDt.ToUniversalTime().ToTimestamp(),
                RejectReason = order.RejectReason,
                RemainingVolume = order.RemainingVolume,
                Side = order.Side switch
                {
                    OrderSide.Unknown => GrpcContract.Orders.OrderSide.Unknown,
                    OrderSide.Buy => GrpcContract.Orders.OrderSide.Buy,
                    OrderSide.Sell => GrpcContract.Orders.OrderSide.Sell,
                    _ => throw new ArgumentOutOfRangeException()
                },
                StatusDt = order.StatusDt.ToUniversalTime().ToTimestamp(),
                Straight = order.Straight,
                UpperLimitPrice = order.UpperLimitPrice,
                UpperPrice = order.UpperPrice
            };
        }

        public static PaginationInt32 EnsurePagination(PaginationInt32 paginationInt32)
        {
            if (paginationInt32 == null)
            {
                return new PaginationInt32()
                {
                    Limit = 1000,
                    Offset = 0
                };
            }

            return paginationInt32;
        }
    }
}
