using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Antares.Service.History.Contracts.Enums;
using Antares.Service.History.Contracts.History;
using JetBrains.Annotations;
using Refit;

namespace Antares.Service.History.Client
{
    /// <summary>
    ///     Orders client API interface.
    /// </summary>
    [PublicAPI]
    public interface ITradesApi
    {
        /// <summary>
        /// Get trades by wallet id
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="assetId"></param>
        /// <param name="assetPairId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="tradeType"></param>
        /// <returns></returns>
        [Get("/api/trades")]
        Task<IEnumerable<TradeModel>> GetTradesByWalletAsync(
            Guid walletId,
            string assetId = null,
            string assetPairId = null,
            int offset = 0,
            int limit = 100,
            DateTime? from = null,
            DateTime? to = null,
            TradeType? tradeType = null);
        
        /// <summary>
        /// Get order trades
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [Get("/api/trades/order/{walletId}/{orderId}")]
        Task<IEnumerable<TradeModel>> GetTradesByOrderIdAsync(Guid walletId, Guid orderId);
    }
}
