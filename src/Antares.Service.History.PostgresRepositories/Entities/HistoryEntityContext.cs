using Antares.Service.History.Core.Domain.Enums;

namespace Antares.Service.History.PostgresRepositories.Entities
{
    internal class HistoryEntityContext
    {
        /// <summary>
        ///     History record state
        /// </summary>
        public HistoryState State { get; set; }

        /// <summary>
        ///     Price (used for trades, orderEvents)
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        ///     Blockchain hash
        /// </summary>
        public string BlockchainHash { get; set; }

        /// <summary>
        ///     Fee size
        /// </summary>
        public decimal? FeeSize { get; set; }

        /// <summary>
        ///     Fee asset
        /// </summary>
        public string FeeAssetId { get; set; }

        /// <summary>
        ///     Quoting asset id for trade searching
        /// </summary>
        public string TradeQuotingAssetId { get; set; }

        /// <summary>
        ///     Quoting volume - necessary for single-side trade view
        /// </summary>
        public decimal TradeQuotingVolume { get; set; }

        /// <summary>
        ///     Trade sequence number
        /// </summary>
        public int TradeIndex { get; set; }

        /// <summary>
        ///     Trade side (maker or taker)
        /// </summary>
        public TradeRole TradeRole { get; set; }

        /// <summary>
        ///     For now can be only  Placed or Cancelled
        /// </summary>
        public OrderStatus OrderEventStatus { get; set; }

        /// <summary>
        ///     Order id
        /// </summary>
        public string OrderId { get; set; }
    }
}
