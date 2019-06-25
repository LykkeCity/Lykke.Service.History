using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Models.Events;
using Lykke.MatchingEngine.Connector.Models.Events.Common;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.History.PostgresRepositories;
using Lykke.Service.History.PostgresRepositories.Repositories;
using Lykke.SettingsReader;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Order = Lykke.Service.History.Core.Domain.Orders.Order;
using OrderStatus = Lykke.Service.History.Core.Domain.Enums.OrderStatus;

namespace Lykke.Service.History.Replay
{
    public class OrdersStateFixer
    {
        private const int BulkSize = 1000;

        private readonly IOrdersRepository _ordersRepository;
        private readonly INoSQLTableStorage<MatchingEngineEvent> _table;

        public OrdersStateFixer(
            ILogFactory logFactory,
            string meConnectionString,
            string historyConnectionString) : this(logFactory, meConnectionString, new OrdersRepository(new ConnectionFactory(historyConnectionString)))
        {
        }

        public OrdersStateFixer(
            ILogFactory logFactory,
            string meConnectionString,
            IOrdersRepository ordersRepository)
        {
            _table = AzureTableStorage<MatchingEngineEvent>.Create(
                new InMemoryReloadingManager(meConnectionString),
                "MatchingEngineClientsEvents0", logFactory);

            _ordersRepository = ordersRepository;
        }
        public async Task FixClosedOrdersState()
        {
            Console.WriteLine("Fixing started");
            await _table.GetDataByChunksAsync(Process);
            Console.WriteLine("Fixing finished");
        }

        //0. skip where "messageType" != "ORDER"
        //1. check if walletId is robot
        //2. group by externalId
        //3. select first by sequenceNumber desc

        //5. compare order status
        //6. update orders where statuses are not equal
        
        public async Task Process(IEnumerable<MatchingEngineEvent> events)
        {
            var ordersBatch = new List<Order>();
            foreach (var matchingEngineEvent in events)
            {
                try
                {
                    var parsed = JsonConvert.DeserializeObject<ExecutionEvent>(matchingEngineEvent.Message);

                    if (parsed.Header.MessageType != MessageType.Order)
                        continue;

                    var orders = parsed.Orders.Select(x => new Order
                    {
                        SequenceNumber = parsed.Header.SequenceNumber,
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
                        RemainingVolume = ParseNullabe(x.RemainingVolume).Value,
                        Side = (Core.Domain.Enums.OrderSide)(int)x.Side,
                        Status = (Core.Domain.Enums.OrderStatus)(int)x.Status,
                        StatusDt = x.StatusDate,
                        Straight = x.OrderType == OrderType.Limit || x.OrderType == OrderType.StopLimit || x.Straight,
                        Type = (Core.Domain.Enums.OrderType)(int)x.OrderType,
                        UpperLimitPrice = ParseNullabe(x.UpperLimitPrice),
                        UpperPrice = ParseNullabe(x.UpperPrice),
                        Trades = x.Trades?.Select(t => new Core.Domain.History.Trade
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
                            Role = (Core.Domain.Enums.TradeRole)(int)t.Role,
                            FeeSize = ParseNullabe(t.Fees?.FirstOrDefault()?.Volume),
                            FeeAssetId = t.Fees?.FirstOrDefault()?.AssetId
                        }).ToList()
                    }).ToList();

                    foreach (var order in orders.Where(x =>
                        x.Status == OrderStatus.Cancelled
                        || x.Status == OrderStatus.Matched
                        || x.Status == OrderStatus.Rejected
                        || x.Status == OrderStatus.Replaced))
                    {
                        ordersBatch.Add(order);
                    }
                }
                catch (Exception)
                {

                }
            }

            if (ordersBatch.Count > 0)
            {
                Console.WriteLine($"Orders count: {ordersBatch.Count}");
                await _ordersRepository.UpsertBulkAsync(ordersBatch);
            }
        }

        private decimal? ParseNullabe(string value)
        {
            return !string.IsNullOrEmpty(value) ? decimal.Parse(value) : (decimal?)null;
        }
    }

    public class InMemoryReloadingManager : IReloadingManager<string>
    {
        private readonly string _connString;

        public InMemoryReloadingManager(string connString)
        {
            _connString = connString;
        }

        public Task<string> Reload()
        {
            return Task.FromResult(_connString);
        }

        public bool WasReloadedFrom(DateTime dateTime)
        {
            return true;
        }

        public bool HasLoaded => true;
        public string CurrentValue => _connString;
    }
}
