using System;
using Antares.Service.History.Core.Domain.Enums;

namespace Antares.Service.History.Core.Domain.History
{
    public class Trade : BaseHistoryRecord
    {
        public string AssetPairId { get; set; }

        public decimal BaseVolume { get; set; }

        public string BaseAssetId { get; set; }

        public decimal Price { get; set; }

        public decimal? FeeSize { get; set; }

        public string FeeAssetId { get; set; }

        public int Index { get; set; }

        public TradeRole Role { get; set; }

        public string QuotingAssetId { get; set; }

        public decimal QuotingVolume { get; set; }

        public Guid OrderId { get; set; }

        public override HistoryType Type => HistoryType.Trade;
    }
}
