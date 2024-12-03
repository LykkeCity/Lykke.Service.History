using System;
using System.ComponentModel.DataAnnotations.Schema;
using Antares.Service.History.Core.Domain.Enums;

namespace Antares.Service.History.PostgresRepositories.Entities
{
    [Table(Constants.OrdersTableName)]
    internal class OrderEntity
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("matching_id")]
        public Guid MatchingId { get; set; }

        [Column("wallet_id")]
        public Guid WalletId { get; set; }

        [Column("type")]
        public OrderType Type { get; set; }

        [Column("side")]
        public OrderSide Side { get; set; }

        [Column("status")]
        public OrderStatus Status { get; set; }

        [Column("assetpair_id")]
        public string AssetPairId { get; set; }

        [Column("volume")]
        public decimal Volume { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }

        [Column("create_dt")]
        public DateTime CreateDt { get; set; }

        [Column("register_dt")]
        public DateTime RegisterDt { get; set; }

        [Column("status_dt")]
        public DateTime StatusDt { get; set; }

        [Column("match_dt")]
        public DateTime? MatchDt { get; set; }

        [Column("remaining_volume")]
        public decimal RemainingVolume { get; set; }

        [Column("reject_reason")]
        public string RejectReason { get; set; }

        [Column("lower_limit_price")]
        public decimal? LowerLimitPrice { get; set; }

        [Column("lower_price")]
        public decimal? LowerPrice { get; set; }

        [Column("upper_limit_price")]
        public decimal? UpperLimitPrice { get; set; }

        [Column("upper_price")]
        public decimal? UpperPrice { get; set; }

        [Column("straight")]
        public bool Straight { get; set; }

        [Column("sequence_number")]
        public long SequenceNumber { get; set; }
    }
}
