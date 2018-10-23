﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.History.Core.Domain.Enums;

namespace Lykke.Service.History.Core.Domain.History
{
    public interface IHistoryRecordsRepository
    {
        Task<BaseHistoryRecord> Get(Guid id, Guid walletId);

        Task<bool> InsertBulkAsync(IEnumerable<BaseHistoryRecord> records);

        Task<bool> TryInsertAsync(BaseHistoryRecord entity);

        Task<bool> TryDeleteAsync(Guid operationId, Guid walletId);

        Task<bool> UpdateBlockchainHashAsync(Guid id, string hash);

        Task<IEnumerable<BaseHistoryRecord>> GetByWallet(Guid walletId, HistoryType[] type, int offset, int limit,
            string assetpairId = null, string assetId = null, DateTime? fromDt = null, DateTime? toDt = null);

        Task<IEnumerable<Trade>> GetTradesByWallet(Guid walletId, int offset,
            int limit, string assetPairId = null, string assetId = null, DateTime? fromDt = null, DateTime? toDt = null,
            bool? buyTrades = null);
    }
}
