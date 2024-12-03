using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Antares.Service.History.Contracts.Enums;
using Antares.Service.History.Contracts.Orders;
using JetBrains.Annotations;
using Refit;

namespace Antares.Service.History.Client
{
    /// <summary>
    ///     Orders client API interface.
    /// </summary>
    [PublicAPI]
    public interface IOrdersApi
    {
        /// <summary>
        /// Get order by id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [Get("/api/orders/{orderId}")]
        Task<OrderModel> GetOrderAsync(Guid orderId);

        /// <summary>
        /// Get order list by wallet id
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="status"></param>
        /// <param name="type"></param>
        /// <param name="assetPairId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [Get("/api/orders/list")]
        Task<IEnumerable<OrderModel>> GetOrdersByWalletAsync(Guid walletId, [Query(CollectionFormat.Multi)] OrderStatus[] status = null, [Query(CollectionFormat.Multi)] OrderType[] type = null, int offset = 0, int limit = 100, string assetPairId = null);

        /// <summary>
        /// Get active orders by wallet id
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="assetPairId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [Get("/api/orders/active")]
        Task<IEnumerable<OrderModel>> GetActiveOrdersByWalletAsync(Guid walletId, int offset = 0, int limit = 100, string assetPairId = null);
    }
}
