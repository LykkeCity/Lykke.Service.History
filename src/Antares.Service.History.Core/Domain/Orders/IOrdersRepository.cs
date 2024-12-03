using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Antares.Service.History.Core.Domain.Enums;
using Antares.Service.History.Core.Domain.History;

namespace Antares.Service.History.Core.Domain.Orders
{
    public interface IOrdersRepository
    {
        Task UpsertBulkAsync(IEnumerable<Order> records);

        Task<bool> InsertOrUpdateAsync(Order order);

        Task<Order> GetAsync(Guid id);

        Task<IEnumerable<Order>> GetOrdersAsync(
            Guid walletId,
            OrderType[] types,
            OrderStatus[] statuses,
            string assetPairId,
            int offset,
            int limit);

        Task<IEnumerable<Order>> GetOrdersByDatesAsync(
            DateTime from,
            DateTime to,
            int offset,
            int limit);

        Task<IEnumerable<Trade>> GetTradesByOrderIdAsync(Guid walletId, Guid id);
    }
}
