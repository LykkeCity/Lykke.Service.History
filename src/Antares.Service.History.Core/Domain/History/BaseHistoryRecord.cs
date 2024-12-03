using System;
using Antares.Service.History.Core.Domain.Enums;

namespace Antares.Service.History.Core.Domain.History
{
    public abstract class BaseHistoryRecord
    {
        public abstract HistoryType Type { get; }

        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
