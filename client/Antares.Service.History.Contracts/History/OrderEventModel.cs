using System;
using Antares.Service.History.Contracts.Enums;

namespace Antares.Service.History.Contracts.History
{
    public class OrderEventModel : BaseHistoryModel
    {
        public override HistoryType Type => HistoryType.OrderEvent;

        public decimal Volume { get; set; }

        public string AssetPairId { get; set; }

        public Guid OrderId { get; set; }

        public decimal Price { get; set; }

        public OrderStatus Status { get; set; }
    }
}
