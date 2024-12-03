using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Antares.Service.History.Contracts.Enums;
using Antares.Service.History.Contracts.History;
using JetBrains.Annotations;
using Refit;

namespace Antares.Service.History.Client
{
    /// <summary>
    ///     History client API interface.
    /// </summary>
    [PublicAPI]
    public interface IHistoryApi
    {
        /// <summary>
        /// Get history by wallet id
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="type"></param>
        /// <param name="assetId"></param>
        /// <param name="assetPairId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        [Get("/api/history")]
        Task<IEnumerable<BaseHistoryModel>> GetHistoryByWalletAsync(Guid walletId,
            [Query(CollectionFormat.Multi)] HistoryType[] type = null,
            string assetId = null,
            string assetPairId = null,
            int offset = 0,
            int limit = 100,
            DateTime? from = null,
            DateTime? to = null);

        /// <summary>
        /// Get history item by id
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Get("/api/history/{walletId}/{id}")]
        Task<BaseHistoryModel> GetHistoryItemByIdAsync(Guid walletId, Guid id);
    }
}
