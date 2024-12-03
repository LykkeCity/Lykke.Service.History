using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antares.Service.History.Core.Domain.Enums;
using Antares.Service.History.Core.Domain.History;
using AutoMapper;

namespace Antares.Service.History.Tests.Init
{
    public class InMemoryHistoryRepository : IHistoryRecordsRepository
    {
        private readonly List<BaseHistoryRecord> _data = new List<BaseHistoryRecord>();

        public Task<BaseHistoryRecord> GetAsync(Guid id, Guid walletId)
        {
            return Task.FromResult(_data.FirstOrDefault(x => x.Id == id && x.WalletId == walletId));
        }

        public Task<bool> InsertBulkAsync(IEnumerable<BaseHistoryRecord> records)
        {
            if (_data.Any(x => records.Any(o => o.Id == x.Id)))
                Task.FromResult(false);

            _data.AddRange(records);

            return Task.FromResult(true);
        }

        public async Task<bool> TryInsertAsync(BaseHistoryRecord entity)
        {
            if (await GetAsync(entity.Id, entity.WalletId) == null)
            {
                _data.Add(entity);
                return true;
            }

            return false;
        }

        public async Task<bool> TryDeleteAsync(Guid operationId, Guid walletId)
        {
            var item = await GetAsync(operationId, walletId);
            if (item == null)
                return false;

            _data.Remove(item);
            return true;
        }

        public Task<bool> UpdateBlockchainHashAsync(Guid id, string hash)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BaseHistoryRecord>> GetByWalletAsync(
            Guid walletId,
            HistoryType[] type,
            int offset,
            int limit,
            string assetPairId = null,
            string assetId = null,
            DateTime? fromDt = null,
            DateTime? toDt = null)
        {
            var typesMap = new Dictionary<HistoryType, Type>
            {
                {HistoryType.CashIn, typeof(Cashin)},
                {HistoryType.CashOut, typeof(Cashout)},
                {HistoryType.Trade, typeof(Trade)},
                {HistoryType.OrderEvent, typeof(OrderEvent)}
            };

            var neededTypes = type.Select(x => typesMap[x]);

            return Task.FromResult(_data.Where(x => x.WalletId == walletId && neededTypes.Contains(x.GetType()))
                .Where(x => string.IsNullOrWhiteSpace(assetPairId) || ((dynamic)x).AssetPairId == assetPairId)
                .Where(x => string.IsNullOrWhiteSpace(assetId) || ((dynamic)x).BaseAssetId == assetId ||
                            ((dynamic)x).QuotingAssetId == assetId)
                .OrderByDescending(x => x.Timestamp)
                .Skip(offset)
                .Take(limit));
        }

        public Task<IEnumerable<Trade>> GetTradesByWalletAsync(
            Guid walletId,
            int offset,
            int limit,
            string assetPairId = null,
            string assetId = null,
            DateTime? fromDt = null,
            DateTime? toDt = null,
            bool? buyTrades = null)
        {
            var items = _data.Where(x => x.WalletId == walletId)
                .Where(x => string.IsNullOrWhiteSpace(assetPairId) || ((dynamic) x).AssetPairId == assetPairId)
                .Where(x => string.IsNullOrWhiteSpace(assetId) || ((dynamic) x).BaseAssetId == assetId ||
                            ((dynamic) x).QuotingAssetId == assetId)
                .OrderByDescending(x => x.Timestamp)
                .Skip(offset)
                .Take(limit);

            return Task.FromResult(Mapper.Map<IEnumerable<Trade>>(items));
        }

        public Task<IEnumerable<Trade>> GetByDatesAsync(
            DateTime fromDt,
            DateTime toDt,
            int offset,
            int limit)
        {
            var items = _data
                .Where(x => x.Timestamp >= fromDt && x.Timestamp < toDt)
                .OrderBy(x => x.Timestamp)
                .Skip(offset)
                .Take(limit);

            return Task.FromResult(Mapper.Map<IEnumerable<Trade>>(items));
        }
    }
}
