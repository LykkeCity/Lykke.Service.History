using Antares.Service.History.Core.Domain.Enums;

namespace Antares.Service.History.Core.Domain.History
{
    public class Cashin : BaseHistoryRecord
    {
        public decimal Volume { get; set; }

        public string AssetId { get; set; }

        public string BlockchainHash { get; set; }

        public HistoryState State { get; set; }

        public decimal? FeeSize { get; set; }

        public override HistoryType Type => HistoryType.CashIn;
    }
}
