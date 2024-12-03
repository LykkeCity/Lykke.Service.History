using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antares.Service.History.Core.Domain.Enums;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.Core.Domain.Orders;
using Antares.Service.History.Tests.Init;
using Autofac;
using MoreLinq;
using Xunit;

namespace Antares.Service.History.Tests
{
    [Collection("history-tests")]
    public class PostgresTests
    {
        public PostgresTests(TestInitialization initialization)
        {
            _container = initialization.Container;
        }

        private readonly IContainer _container;


        private async Task<(Guid, List<Core.Domain.History.BaseHistoryRecord>)> GenerateData(string[] pairs, string[] assets)
        {
            var walletId = Guid.NewGuid();
            var random = new Random();

            const int cashinsCount = 5;
            const int cashoutsCount = 3;
            const int tradesCount = 20;

            var list = new List<BaseHistoryRecord>();

            for (var i = 0; i < cashinsCount; i++)
            {
                var cashin = new Cashin
                {
                    Id = Guid.NewGuid(),
                    WalletId = walletId,
                    AssetId = assets[random.Next(assets.Length)],
                    State = HistoryState.Finished,
                    Timestamp = DateTime.UtcNow,
                    Volume = random.Next(1, 100)
                };

                list.Add(cashin);
            }

            for (var i = 0; i < cashoutsCount; i++)
            {
                var cashout = new Cashout
                {
                    Id = Guid.NewGuid(),
                    WalletId = walletId,
                    AssetId = assets[random.Next(assets.Length)],
                    State = HistoryState.Finished,
                    Timestamp = DateTime.UtcNow,
                    Volume = -random.Next(1, 100)
                };

                list.Add(cashout);
            }

            for (var i = 0; i < tradesCount; i++)
            {
                var cashout = new Trade
                {
                    Id = Guid.NewGuid(),
                    WalletId = walletId,
                    BaseAssetId = assets[random.Next(assets.Length)],
                    Timestamp = DateTime.UtcNow,
                    BaseVolume = -random.Next(1, 100),
                    QuotingAssetId = assets[random.Next(assets.Length)],
                    QuotingVolume = random.Next(1, 50),
                    AssetPairId = pairs[random.Next(pairs.Length)],
                    OrderId = Guid.NewGuid(),
                    Price = random.Next() * 100
                };

                list.Add(cashout);
            }

            var repo = _container.Resolve<IHistoryRecordsRepository>();

            await repo.InsertBulkAsync(list);

            return (walletId, list);
        }

        private Order GetOrder()
        {
            var walletId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var date = DateTime.UtcNow;

            var random = new Random();

            Trade GetTrade()
            {
                return new Trade
                {
                    Id = Guid.NewGuid(),
                    WalletId = walletId,
                    AssetPairId = "BTCUSD",
                    BaseAssetId = "BTC",
                    BaseVolume = 5,
                    QuotingAssetId = "USD",
                    QuotingVolume = -5002,
                    Price = 10001,
                    Timestamp = date.AddMilliseconds(1)
                };
            }

            var orderModel = new Order
            {
                Id = orderId,
                AssetPairId = "BTCUSD",
                CreateDt = date,
                StatusDt = date,
                MatchingId = Guid.NewGuid(),
                Price = random.Next() * 100,
                Volume = random.Next() * 10,
                RemainingVolume = 10,
                Side = OrderSide.Buy,
                Status = (OrderStatus) random.Next(0, 8),
                Straight = true,
                Type = (OrderType) random.Next(0, 4),
                WalletId = walletId,
                Trades = Enumerable.Range(1, random.Next(1, 20)).Select(x => GetTrade()).ToArray()
            };

            return orderModel;
        }

        [Fact]
        public async Task HistorySelect_Test()
        {
            var pairs = new[] {"BTCUSD", "EURUSD", "ETHUSD"};
            var assets = new[] {"BTC", "USD", "EUR", "ETH"};

            var repo = _container.Resolve<IHistoryRecordsRepository>();

            var (walletId, data) = await GenerateData(pairs, assets);

            var q1 = await repo.GetByWalletAsync(walletId, new[] {HistoryType.CashIn, HistoryType.CashOut}, 0, 100);

            Assert.Equal(data.OfType<Cashin>().Count() + data.OfType<Cashout>().Count(), q1.Count());

            var q2 = await repo.GetByWalletAsync(walletId, new[] {HistoryType.Trade}, 0, 100);

            Assert.Equal(data.OfType<Trade>().Count(), q2.Count());

            var q3 = await repo.GetByWalletAsync(walletId,
                new[] {HistoryType.CashIn, HistoryType.CashOut, HistoryType.Trade}, 0, 1);

            Assert.Single(q3);

            var q4 = await repo.GetByWalletAsync(walletId, new[] {HistoryType.Trade}, 0, 100, assetId: "USD");

            Assert.Equal(data.OfType<Trade>().Count(x => x.BaseAssetId == "USD" || x.QuotingAssetId == "USD"), q4.Count());

            var q5 = await repo.GetByWalletAsync(walletId, new[] {HistoryType.Trade}, 0, 100, "BTCUSD");

            Assert.Equal(data.OfType<Trade>().Count(x => x.AssetPairId == "BTCUSD"), q5.Count());
        }

        [Fact]
        public async Task InsertOrdersWithSameId_Test()
        {
            var repo = _container.Resolve<IOrdersRepository>();
            var repotrades = _container.Resolve<IHistoryRecordsRepository>();

            var orderId = Guid.NewGuid();
            var orders = Enumerable.Range(1, 10).Select(x => GetOrder()).ToList();

            orders.ForEach((o, idx) =>
            {
                o.SequenceNumber = idx;
                o.Id = orderId;
            });

            await repo.UpsertBulkAsync(orders);

            await repotrades.InsertBulkAsync(orders.SelectMany(x => x.Trades));

            var newestOrder = orders.OrderByDescending(x => x.SequenceNumber).First();
            var orderFromRepo = await repo.GetAsync(orderId);

            Assert.Equal(newestOrder.SequenceNumber, orderFromRepo.SequenceNumber);
            Assert.Equal(newestOrder.Status, orderFromRepo.Status);
            Assert.Equal(newestOrder.Type, orderFromRepo.Type);
        }

        [Fact]
        public async Task UpdateOrderOnlyWithBiggerSequence_Test()
        {
            var repo = _container.Resolve<IOrdersRepository>();

            var orderId = Guid.NewGuid();
            var order = GetOrder();

            order.Id = orderId;

            order.SequenceNumber = 1;
            order.Status = OrderStatus.Cancelled;

            await repo.InsertOrUpdateAsync(order);

            var orderFromRepo = await repo.GetAsync(orderId);

            Assert.Equal(OrderStatus.Cancelled, orderFromRepo.Status);

            order.SequenceNumber = 1;
            order.Status = OrderStatus.Rejected;

            await repo.InsertOrUpdateAsync(order);

            orderFromRepo = await repo.GetAsync(orderId);

            Assert.Equal(OrderStatus.Cancelled, orderFromRepo.Status);

            order.SequenceNumber = 2;
            order.Status = OrderStatus.Matched;

            await repo.InsertOrUpdateAsync(order);

            orderFromRepo = await repo.GetAsync(orderId);

            Assert.Equal(OrderStatus.Matched, orderFromRepo.Status);
        }
    }
}
