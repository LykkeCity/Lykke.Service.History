using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Antares.Service.History.Contracts.Enums;
using Antares.Service.History.Contracts.History;
using Antares.Service.History.Core.Domain.History;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Antares.Service.History.Controllers
{
    [Route("api/history")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;

        public HistoryController(IHistoryRecordsRepository historyRecordsRepository)
        {
            _historyRecordsRepository = historyRecordsRepository;
        }

        /// <summary>
        /// Get wallet history
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
        [HttpGet("")]
        [SwaggerOperation("GetHistory")]
        [ProducesResponseType(typeof(IReadOnlyList<BaseHistoryModel>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<BaseHistoryModel>> GetHistory(
            [FromQuery] Guid walletId,
            [FromQuery(Name = "type")] HistoryType[] type,
            [FromQuery] string assetId = null,
            [FromQuery] string assetPairId = null,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 100,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            if (type.Length == 0)
                type = Enum.GetValues(typeof(HistoryType)).Cast<HistoryType>().ToArray();

            var mappedType = type.Select(x => x switch
            {
                HistoryType.CashIn => Antares.Service.History.Core.Domain.Enums.HistoryType.CashIn,
                HistoryType.CashOut => Antares.Service.History.Core.Domain.Enums.HistoryType.CashOut,
                HistoryType.Trade => Antares.Service.History.Core.Domain.Enums.HistoryType.Trade,
                HistoryType.OrderEvent => Antares.Service.History.Core.Domain.Enums.HistoryType.OrderEvent,

                _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
            }).ToArray();
            var data = await _historyRecordsRepository.GetByWalletAsync(walletId, mappedType, 
                offset, 
                limit, 
                assetPairId, 
                assetId, 
                from, 
                to);

            return Mapper.Map<IReadOnlyList<BaseHistoryModel>>(data);
        }

        /// <summary>
        /// Get wallet history
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{walletId}/{id}")]
        [SwaggerOperation("GetHistoryItem")]
        [ProducesResponseType(typeof(BaseHistoryModel), (int)HttpStatusCode.OK)]
        public async Task<BaseHistoryModel> GetHistory(Guid walletId, Guid id)
        {
            var data = await _historyRecordsRepository.GetAsync(id, walletId);

            return Mapper.Map<BaseHistoryModel>(data);
        }
    }
}


