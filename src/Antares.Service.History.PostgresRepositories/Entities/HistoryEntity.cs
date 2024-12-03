using System;
using System.ComponentModel.DataAnnotations.Schema;
using Antares.Service.History.Core.Domain.Enums;
using Common;
using Newtonsoft.Json;

namespace Antares.Service.History.PostgresRepositories.Entities
{
    [Table(Constants.HistoryTableName)]
    internal class HistoryEntity
    {
        [Column("type")]
        public HistoryType Type { get; set; }

        [Column("id")]
        public Guid Id { get; set; }

        [Column("wallet_id")]
        public Guid WalletId { get; set; }

        [Column("asset_id")]
        public string AssetId { get; set; }

        [Column("assetpair_id")]
        public string AssetPairId { get; set; }

        [Column("volume")]
        public decimal Volume { get; set; }

        [Column("create_dt")]
        public DateTime Timestamp { get; set; }

        [Column("context", TypeName = "jsonb")]
        public string Context
        {
            get => ContextObject == null
                ? null
                : JsonConvert.SerializeObject(ContextObject, new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });
            set => ContextObject = value?.DeserializeJson<HistoryEntityContext>();
        }

        [NotMapped]
        public HistoryEntityContext ContextObject { get; set; }
    }
}
