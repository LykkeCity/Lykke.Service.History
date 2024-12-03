using Antares.Service.History.PostgresRepositories.Entities;
using PostgreSQLCopyHelper;

namespace Antares.Service.History.PostgresRepositories.Mappings
{
    internal class HistoryEntityBulkMapping
    {
        public static PostgreSQLCopyHelper<HistoryEntity> Generate()
        {
            return new PostgreSQLCopyHelper<HistoryEntity>(Constants.HistorySchemaName, Constants.HistoryTableName)
                .MapUUID("id", x => x.Id)
                .MapUUID("wallet_id", x => x.WalletId)
                .MapText("asset_id", x => x.AssetId)
                .MapText("assetpair_id", x => x.AssetPairId)
                .MapNumeric("volume", x => x.Volume)
                .MapInteger("type", x => (int) x.Type)
                .MapTimeStamp("create_dt", x => x.Timestamp)
                .MapJsonb("context", x => x.Context);
        }
    }
}
