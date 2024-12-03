using System;
using Antares.Service.History.Contracts.Enums;

namespace Antares.Service.History.Contracts.History
{
    public abstract class BaseHistoryModel
    {
        public abstract HistoryType Type { get; }

        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
