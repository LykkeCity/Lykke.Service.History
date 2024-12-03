using Antares.Service.History.Contracts.Enums;

namespace Antares.Service.History.Contracts.History
{
    public class CashoutModel : BaseHistoryModel
    {
        public override HistoryType Type => HistoryType.CashOut;

        public decimal Volume { get; set; }

        public string AssetId { get; set; }

        public string BlockchainHash { get; set; }

        public HistoryState State { get; set; }

        public decimal? FeeSize { get; set; }
    }
}
