using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.History.Contracts.Enums;
using Lykke.Service.History.Contracts.History;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Lykke.Service.History.Controllers
{
    [Route("api/trades")]
    [ApiController]
    public class TradesController
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IHistoryRecordsRepository _historyRecordsRepository;

        public TradesController(
            IOrdersRepository ordersRepository,
            IHistoryRecordsRepository historyRecordsRepository)
        {
            _ordersRepository = ordersRepository;
            _historyRecordsRepository = historyRecordsRepository;
        }

        /// <summary>
        /// Get wallet trades
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="assetId"></param>
        /// <param name="assetPairId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="tradeType"></param>
        /// <returns></returns>
        [HttpGet("")]
        [SwaggerOperation("GetTrades")]
        [ProducesResponseType(typeof(IReadOnlyList<TradeModel>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<TradeModel>> GetTrades(
            [FromQuery] Guid walletId,
            [FromQuery] string assetId = null,
            [FromQuery] string assetPairId = null,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 100,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery(Name = "tradeType")] TradeType? tradeType = null)
        {
            bool? onlyBuyTrades = null;
            if (tradeType == TradeType.Buy)
                onlyBuyTrades = true;
            if (tradeType == TradeType.Sell)
                onlyBuyTrades = false;

            var data = await _historyRecordsRepository.GetTradesByWalletAsync(walletId, offset, limit, assetPairId, assetId, from, to, onlyBuyTrades);

            return Mapper.Map<IReadOnlyList<TradeModel>>(data);
        }

        /// <summary>
        /// Get order trades
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("order/{walletId}/{id}")]
        [SwaggerOperation("GetOrderTrades")]
        [ProducesResponseType(typeof(IReadOnlyList<TradeModel>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<TradeModel>> GetTradesByOrderId(Guid walletId, Guid id)
        {
            var trades = await _ordersRepository.GetTradesByOrderIdAsync(walletId, id);
            return Mapper.Map<IReadOnlyList<TradeModel>>(trades);
        }

        /// <summary>
        /// Get wallet trades by dates range.
        /// </summary>
        /// <param name="from">Inclusive range start</param>
        /// <param name="to">Exclusive range finish</param>
        /// <param name="offset">Number of skipped elements</param>
        /// <param name="limit">Max number of elements to be fetched</param>
        /// <returns>Trades from specified range.</returns>
        [HttpGet("bydates/{from}/{to}")]
        [SwaggerOperation("GetTrades")]
        [ProducesResponseType(typeof(IReadOnlyList<TradeModel>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<TradeModel>> GetTradesByDates(
            DateTime from,
            DateTime to,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 100)
        {
            if (from >= to)
                throw new ArgumentException($"Parameter '{nameof(from)}' must be earlier than parameter '{nameof(to)}'");

            var data = await _historyRecordsRepository.GetByDatesAsync(
                from,
                to,
                offset,
                limit);

            return Mapper.Map<IReadOnlyList<TradeModel>>(data);
        }
    }
}
