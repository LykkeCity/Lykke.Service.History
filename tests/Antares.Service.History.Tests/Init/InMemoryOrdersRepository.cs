using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antares.Service.History.Core.Domain.Enums;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.Core.Domain.Orders;
using Common;
using Newtonsoft.Json;

namespace Antares.Service.History.Tests.Init
{
    public class InMemoryOrdersRepository : IOrdersRepository
    {
        private readonly List<Order> _orders = new List<Order>();

        public async Task UpsertBulkAsync(IEnumerable<Order> records)
        {
            foreach (var order in records)
                await InsertOrUpdateAsync(order);
        }

        public Task<bool> InsertOrUpdateAsync(Order order)
        {
            var current = _orders.FirstOrDefault(x => x.Id == order.Id);

            if (current == null)
            {
                _orders.Add(JsonConvert.DeserializeObject<Order>(order.ToJson()));
                return Task.FromResult(true);
            }
            if (current.SequenceNumber >= order.SequenceNumber)
                return Task.FromResult(false);

            current.Type = order.Type;
            current.Status = order.Status;
            current.Volume = order.Volume;
            current.Price = order.Price;
            current.RegisterDt = order.RegisterDt;
            current.StatusDt = order.StatusDt;
            current.MatchDt = order.MatchDt;
            current.RemainingVolume = order.RemainingVolume;
            current.RejectReason = order.RejectReason;
            current.LowerLimitPrice = order.LowerLimitPrice;
            current.LowerPrice = order.LowerPrice;
            current.UpperLimitPrice = order.UpperLimitPrice;
            current.UpperPrice = order.UpperPrice;
            current.SequenceNumber = order.SequenceNumber;

            return Task.FromResult(true);
        }

        public Task<Order> GetAsync(Guid id)
        {
            return Task.FromResult(_orders.FirstOrDefault(x => x.Id == id));
        }

        public Task<IEnumerable<Order>> GetOrdersAsync(
            Guid walletId,
            OrderType[] types,
            OrderStatus[] statuses,
            string assetPairId,
            int offset,
            int limit)
        {
            return Task.FromResult(_orders
                .Where(x => x.WalletId == walletId && statuses.Contains(x.Status) && types.Contains(x.Type))
                .Where(x => string.IsNullOrWhiteSpace(assetPairId) || x.AssetPairId == assetPairId)
                .Skip(offset).Take(limit));
        }

        public Task<IEnumerable<Order>> GetOrdersByDatesAsync(
            DateTime from,
            DateTime to,
            int offset,
            int limit)
        {
            return Task.FromResult(_orders
                .Where(x => x.CreateDt >= from && x.CreateDt < to)
                .Skip(offset)
                .Take(limit));
        }

        public Task<IEnumerable<Trade>> GetTradesByOrderIdAsync(Guid walletId, Guid id)
        {
            return Task.FromResult(_orders.FirstOrDefault(x => x.Id == id)?.Trades.AsEnumerable());
        }
    }
}
