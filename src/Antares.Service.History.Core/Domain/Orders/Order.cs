using System;
using System.Collections.Generic;
using Antares.Service.History.Core.Domain.Enums;
using Antares.Service.History.Core.Domain.History;

namespace Antares.Service.History.Core.Domain.Orders
{
    public class Order
    {
        public Guid Id { get; set; }

        public Guid MatchingId { get; set; }

        public Guid WalletId { get; set; }

        public OrderType Type { get; set; }

        public OrderSide Side { get; set; }

        public OrderStatus Status { get; set; }

        public string AssetPairId { get; set; }

        public decimal Volume { get; set; }

        public decimal? Price { get; set; }

        public DateTime CreateDt { get; set; }

        public DateTime RegisterDt { get; set; }

        public DateTime StatusDt { get; set; }

        public DateTime? MatchDt { get; set; }

        public decimal RemainingVolume { get; set; }

        public string RejectReason { get; set; }

        public decimal? LowerLimitPrice { get; set; }

        public decimal? LowerPrice { get; set; }

        public decimal? UpperLimitPrice { get; set; }

        public decimal? UpperPrice { get; set; }

        public bool Straight { get; set; }

        public Trade[] Trades { get; set; } = Array.Empty<Trade>();

        public long SequenceNumber { get; set; }
    }
}
