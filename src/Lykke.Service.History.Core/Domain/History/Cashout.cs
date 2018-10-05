﻿using Lykke.Service.History.Core.Domain.Enums;

namespace Lykke.Service.History.Core.Domain.History
{
    public class Cashout : BaseHistoryRecord
    {
        public decimal Volume { get; set; }

        public string AssetId { get; set; }

        public string BlockchainHash { get; set; }

        public HistoryState State { get; set; }

        public decimal? FeeSize { get; set; }

        public HistoryOperationType OperationType { get; set; }

        public override HistoryType Type => HistoryType.CashOut;
    }
}
