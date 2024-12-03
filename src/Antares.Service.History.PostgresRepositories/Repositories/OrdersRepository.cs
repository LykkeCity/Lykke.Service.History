using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Antares.Service.History.Core.Domain.Enums;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.Core.Domain.Orders;
using Antares.Service.History.PostgresRepositories.Entities;
using Antares.Service.History.PostgresRepositories.JsonbQuery;
using Antares.Service.History.PostgresRepositories.Mappings;
using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using PostgreSQLCopyHelper;

namespace Antares.Service.History.PostgresRepositories.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private static readonly PostgreSQLCopyHelper<OrderEntity> BulkMapping;

        private readonly string _bulkUpsertQuery = $@"
insert into {Constants.HistorySchemaName}.{Constants.OrdersTableName}
select * from {Constants.TempOrdersTableName}
ON CONFLICT (id) DO UPDATE
    set type = excluded.type,
        status = excluded.status,
        volume = excluded.volume,
        price = excluded.price,
        register_dt = excluded.register_dt,
        status_dt = excluded.status_dt,
        match_dt = excluded.match_dt,
        remaining_volume = excluded.remaining_volume,
        reject_reason = excluded.reject_reason,
        lower_limit_price = excluded.lower_limit_price,
        lower_price = excluded.lower_price,
        upper_limit_price = excluded.upper_limit_price,
        upper_price = excluded.upper_price,
        sequence_number = excluded.sequence_number
            where {Constants.HistorySchemaName}.{Constants.OrdersTableName}.sequence_number < excluded.sequence_number";

        private readonly ConnectionFactory _connectionFactory;

        //Note: Temp tables are created in their own schema (for each session)
        private readonly string _createTempTableQuery = $@"
create temp table if not exists {Constants.TempOrdersTableName} 
(like {Constants.HistorySchemaName}.{Constants.OrdersTableName})
on commit drop";

        private readonly string _insertOrUpdateQuery = $@"
insert into {Constants.HistorySchemaName}.{Constants.OrdersTableName}(id, matching_id, wallet_id, assetpair_id, type, side, status, volume, price, 
                create_dt, register_dt, status_dt, match_dt, remaining_volume, reject_reason, 
                lower_limit_price, lower_price, upper_limit_price, upper_price, straight, sequence_number)
    values (@Id, @MatchingId, @WalletId, @AssetPairId, @Type, @Side, @Status, @Volume, @Price,
            @CreateDt, @RegisterDt, @StatusDt, @MatchDt, @RemainingVolume, @RejectReason, 
            @LowerLimitPrice, @LowerPrice, @UpperLimitPrice, @UpperPrice, @Straight, @SequenceNumber)
ON CONFLICT (id) DO UPDATE
    set type = @Type,
        status = @Status,
        volume = @Volume,
        price = @Price,
        register_dt = @RegisterDt,
        status_dt = @StatusDt,
        match_dt = @MatchDt,
        remaining_volume = @RemainingVolume,
        reject_reason = @RejectReason,
        lower_limit_price = @LowerLimitPrice,
        lower_price = @LowerPrice,
        upper_limit_price = @UpperLimitPrice,
        upper_price = @UpperPrice,
        sequence_number = @SequenceNumber
            where {Constants.HistorySchemaName}.{Constants.OrdersTableName}.sequence_number < @SequenceNumber";

        private readonly string _ordersDateRangeQuery = $@"SELECT * FROM {Constants.HistorySchemaName}.{Constants.OrdersTableName}
WHERE create_dt >= '{{0}}' AND create_dt < '{{1}}' ORDER BY create_dt
LIMIT {{2}} OFFSET {{3}}";

        static OrdersRepository()
        {
            BulkMapping = OrderEntityBulkMapping.Generate();
            SqlMapper.SetTypeMap(
                typeof(OrderEntity),
                new CustomPropertyTypeMap(
                    typeof(OrderEntity),
                    (type, columnName) =>
                        type.GetProperties().FirstOrDefault(prop =>
                            prop.GetCustomAttributes(false)
                                .OfType<ColumnAttribute>()
                                .Any(attr => attr.Name == columnName))));
        }

        public OrdersRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<bool> InsertOrUpdateAsync(Order order)
        {
            using (var connection = await _connectionFactory.CreateNpgsqlConnection())
            {
                var result = await connection.ExecuteAsync(_insertOrUpdateQuery, order);

                return result > 0;
            }
        }

        public async Task UpsertBulkAsync(IEnumerable<Order> records)
        {
            using (var connection = await _connectionFactory.CreateNpgsqlConnection())
            {
                using (var tx = connection.BeginTransaction())
                {
                    await connection.QueryAsync(_createTempTableQuery);

                    var mapped = records.Select(Mapper.Map<OrderEntity>);

                    // if there are more than one order with the same id, we need to send only latest
                    // otherwise PostgreSQL will throw exception 21000: command cannot affect row a second time
                    var uniqueOrders = mapped.GroupBy(x => x.Id)
                        .Select(x => x.OrderByDescending(o => o.SequenceNumber).First());

                    BulkMapping.SaveAll(connection, uniqueOrders);

                    await connection.QueryAsync(_bulkUpsertQuery);

                    await tx.CommitAsync();
                }
            }
        }

        public async Task<Order> GetAsync(Guid id)
        {
            using (var context = _connectionFactory.CreateDataContext())
            {
                return Mapper.Map<Order>(await context.Orders.FirstOrDefaultAsync(x => x.Id == id));
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync(
            Guid walletId,
            OrderType[] types,
            OrderStatus[] statuses,
            string assetPairId,
            int offset,
            int limit)
        {
            using (var context = _connectionFactory.CreateDataContext())
            {
                var query = context.Orders
                    .Where(x => x.WalletId == walletId && statuses.Contains(x.Status) && types.Contains(x.Type))
                    .Where(x => string.IsNullOrWhiteSpace(assetPairId) || x.AssetPairId == assetPairId)
                    .OrderByDescending(x => x.CreateDt)
                    .Skip(offset)
                    .Take(limit);

                return (await query.ToListAsync()).Select(Mapper.Map<Order>);
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByDatesAsync(
            DateTime from,
            DateTime to,
            int offset,
            int limit)
        {
            if (from >= to)
                return new Order[0];

            using (var connection = await _connectionFactory.CreateNpgsqlConnection())
            {
                var query = string.Format(
                    _ordersDateRangeQuery,
                    from.ToString(Constants.DateTimeFormat),
                    to.ToString(Constants.DateTimeFormat),
                    limit,
                    offset);
                var items = await connection.QueryAsync<OrderEntity>(query);

                return items.Select(Mapper.Map<Order>);
            }
        }

        public async Task<IEnumerable<Trade>> GetTradesByOrderIdAsync(Guid walletId, Guid id)
        {
            using (var context = _connectionFactory.CreateDataContext())
            {
                var query = context.History
                    .Where(x => x.WalletId == walletId &&  x.Type == HistoryType.Trade)
                    .Where(x => x.Context.JsonbPath<string>(nameof(HistoryEntityContext.OrderId)) == id.ToString())
                    .OrderByDescending(x => x.Timestamp);

                return Mapper.Map<IEnumerable<Trade>>(await query.ToListAsync());
            }
        }
    }
}
