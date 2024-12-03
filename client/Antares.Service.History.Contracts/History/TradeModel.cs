using System;
using Antares.Service.History.Contracts.Enums;

namespace Antares.Service.History.Contracts.History
{
    public class TradeModel : BaseHistoryModel
    {
        public override HistoryType Type => HistoryType.Trade;

        public string AssetPairId { get; set; }

        public decimal BaseVolume { get; set; }

        public string BaseAssetId { get; set; }

        public string QuotingAssetId { get; set; }

        public decimal QuotingVolume { get; set; }

        public decimal Price { get; set; }

        public decimal? FeeSize { get; set; }

        public string FeeAssetId { get; set; }

        public int Index { get; set; }

        public TradeRole Role { get; set; }

        public Guid OrderId { get; set; }
    }
}
