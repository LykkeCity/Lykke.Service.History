using Antares.Service.History.Contracts.Enums;

namespace Antares.Service.History.Contracts.History
{
    public class CashinModel : BaseHistoryModel
    {
        public override HistoryType Type => HistoryType.CashIn;

        public decimal Volume { get; set; }

        public string AssetId { get; set; }

        public string BlockchainHash { get; set; }

        public HistoryState State { get; set; }

        public decimal? FeeSize { get; set; }
    }
}
