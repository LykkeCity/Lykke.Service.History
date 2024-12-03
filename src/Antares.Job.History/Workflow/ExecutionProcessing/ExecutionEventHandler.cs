using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.Core.Domain.Orders;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Models.Events;
using MoreLinq;
using Utils = Antares.Service.History.Core.Utils;

namespace Antares.Job.History.Workflow.ExecutionProcessing
{
    public class ExecutionEventHandler
    {
        private const int TradesBulkSize = 5000;

        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly IOrdersRepository _ordersRepository;
        private readonly IReadOnlyList<string> _walletIds;
        private readonly ILog _log;

        public ExecutionEventHandler(
            [NotNull] ILogFactory logFactory,
            IHistoryRecordsRepository historyRecordsRepository,
            IOrdersRepository ordersRepository,
            IReadOnlyList<string> walletIds)
        {
            _log = logFactory.CreateLog(nameof(ExecutionEventHandler));
            _historyRecordsRepository = historyRecordsRepository;
            _ordersRepository = ordersRepository;
            _walletIds = walletIds;
        }

        public async Task HandleAsync(IReadOnlyCollection<ExecutionEvent> executionEvents)
        {
            var pairs = executionEvents.Select(Map).ToArray();
            var orders = pairs.SelectMany(x => x.Orders).ToList();
            var orderEvents = pairs.SelectMany(x => x.OrderEvent).ToList();
            var batchId = Guid.NewGuid().ToString();

            bool needLog = orders.Any(x => _walletIds.Contains(x.WalletId.ToString()));

            foreach (var order in orders.Where(x => _walletIds.Contains(x.WalletId.ToString())))
            {
                _log.Info("Saving order (ProcessBatch)", context: $"order: {new { order.Id, order.Status, order.SequenceNumber, batchId }.ToJson()}");
            }

            await _ordersRepository.UpsertBulkAsync(orders);

            if (needLog)
            {
                _log.Info("Orders have been saved (ProcessBatch)", context: $"batchId: {batchId}");
            }

            var trades = orders.SelectMany(x => x.Trades)?.ToArray();

            if (trades.Any())
            {
                var batched = trades.Batch(TradesBulkSize).ToArray();

                foreach (var tradesBatch in batched)
                    await _historyRecordsRepository.InsertBulkAsync(tradesBatch);
            }

            foreach (var batch in orderEvents.Batch(TradesBulkSize))
                await _historyRecordsRepository.InsertBulkAsync(batch);
        }

        private (IReadOnlyCollection<Antares.Service.History.Core.Domain.Orders.Order> Orders, IReadOnlyCollection<OrderEvent> OrderEvent)
            Map(ExecutionEvent executionEvent)
        {
            var orders = new List<Antares.Service.History.Core.Domain.Orders.Order>(executionEvent.Orders.Count);

            foreach (var x in executionEvent.Orders)
            {
                Antares.Service.History.Core.Domain.Orders.Order order;

                try
                {
                    order = new Antares.Service.History.Core.Domain.Orders.Order
                    {
                        Id = Guid.Parse(x.ExternalId),
                        WalletId = Guid.Parse(x.WalletId),
                        Volume = decimal.Parse(x.Volume),
                        AssetPairId = x.AssetPairId,
                        CreateDt = x.CreatedAt,
                        LowerLimitPrice = ParseNullabe(x.LowerLimitPrice),
                        LowerPrice = ParseNullabe(x.LowerPrice),
                        MatchDt = x.LastMatchTime,
                        MatchingId = Guid.Parse(x.Id),
                        Price = ParseNullabe(x.Price),
                        RegisterDt = x.Registered,
                        RejectReason = x.RejectReason,
                        RemainingVolume = ParseNullabe(x.RemainingVolume) ?? 0m,
                        Side = (Antares.Service.History.Core.Domain.Enums.OrderSide) (int) x.Side,
                        Status = (Antares.Service.History.Core.Domain.Enums.OrderStatus) (int) x.Status,
                        StatusDt = x.StatusDate,
                        Straight = x.OrderType == Lykke.MatchingEngine.Connector.Models.Events.OrderType.Limit ||
                                   x.OrderType == Lykke.MatchingEngine.Connector.Models.Events.OrderType.StopLimit ||
                                   x.Straight,
                        SequenceNumber = executionEvent.Header.SequenceNumber,
                        Type = (Antares.Service.History.Core.Domain.Enums.OrderType) (int) x.OrderType,
                        UpperLimitPrice = ParseNullabe(x.UpperLimitPrice),
                        UpperPrice = ParseNullabe(x.UpperPrice),
                    };

                    order.Trades = x.Trades == null
                        ? Array.Empty<Antares.Service.History.Core.Domain.History.Trade>()
                        : x.Trades.Select(t => new Antares.Service.History.Core.Domain.History.Trade()
                        {
                            Id = Guid.Parse(t.TradeId),
                            WalletId = Guid.Parse(x.WalletId),
                            AssetPairId = x.AssetPairId,
                            BaseAssetId = t.BaseAssetId,
                            BaseVolume = decimal.Parse(t.BaseVolume),
                            Price = decimal.Parse(t.Price),
                            Timestamp = t.Timestamp,
                            QuotingAssetId = t.QuotingAssetId,
                            QuotingVolume = decimal.Parse(t.QuotingVolume),
                            Index = t.Index,
                            Role = (Antares.Service.History.Core.Domain.Enums.TradeRole) (int) t.Role,
                            FeeSize = ParseNullabe(t.Fees?.FirstOrDefault()?.Volume),
                            FeeAssetId = t.Fees?.FirstOrDefault()?.AssetId,
                            OrderId = order.Id
                            //OppositeWalletId = Guid.Parse(t.OppositeWalletId),
                        }).ToArray();

                    orders.Add(order);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Can't convert execution event to order", context: $"execution event: {x.ToJson()}");
                }
            }

            foreach (var order in orders.Where(x => _walletIds.Contains(x.WalletId.ToString())))
            {
                _log.Info("Order from ME", $"order: {new { order.Id, order.Status, executionEvent.Header.SequenceNumber }.ToJson()}");
            }

            var limitOrders = orders.Where(x => x.Type == Antares.Service.History.Core.Domain.Enums.OrderType.Limit ||
                                                x.Type == Antares.Service.History.Core.Domain.Enums.OrderType.StopLimit).ToList();
            var historyOrders = new List<OrderEvent>(limitOrders.Count);

            foreach (var order in limitOrders)
            {
                switch (order.Status)
                {
                    case Service.History.Core.Domain.Enums.OrderStatus.Cancelled:
                        {
                            var orderEvent = new OrderEvent
                            {
                                OrderId = order.Id,
                                Status = order.Status,
                                AssetPairId = order.AssetPairId,
                                Price = order.Price ?? 0,
                                Timestamp = order.StatusDt,
                                Volume = order.Volume,
                                WalletId = order.WalletId,
                                Id = Utils.IncrementGuid(order.Id, (int)Service.History.Core.Domain.Enums.OrderStatus.Cancelled)
                            };

                            historyOrders.Add(orderEvent);

                            break;
                        }
                    case Service.History.Core.Domain.Enums.OrderStatus.Placed:
                        {
                            var orderEvent = new OrderEvent
                            {
                                OrderId = order.Id,
                                Status = order.Status,
                                AssetPairId = order.AssetPairId,
                                Price = order.Price ?? 0,
                                Timestamp = order.CreateDt,
                                Volume = order.Volume,
                                WalletId = order.WalletId,
                                Id = Utils.IncrementGuid(order.Id, (int)Service.History.Core.Domain.Enums.OrderStatus.Placed)
                            };

                            historyOrders.Add(orderEvent);

                            break;
                        }
                    case Service.History.Core.Domain.Enums.OrderStatus.Matched:
                    case Service.History.Core.Domain.Enums.OrderStatus.PartiallyMatched:
                        {
                            if (!order.Trades.Any(t => t.Role == Service.History.Core.Domain.Enums.TradeRole.Taker))
                                break;
                            var orderEvent = new OrderEvent()
                            {
                                OrderId = order.Id,
                                Status = Service.History.Core.Domain.Enums.OrderStatus.Placed,
                                AssetPairId = order.AssetPairId,
                                Price = order.Price ?? 0,
                                Timestamp = order.CreateDt,
                                Volume = order.Volume,
                                WalletId = order.WalletId,
                                Id = Utils.IncrementGuid(order.Id, (int)Service.History.Core.Domain.Enums.OrderStatus.Placed)
                            };

                            historyOrders.Add(orderEvent);

                            break;
                        }
                    default:
                        continue;
                }
            }

            return (orders, historyOrders);
        }

        private decimal? ParseNullabe(string value)
        {
            return !string.IsNullOrEmpty(value) ? decimal.Parse(value) : (decimal?)null;
        }
    }
}
