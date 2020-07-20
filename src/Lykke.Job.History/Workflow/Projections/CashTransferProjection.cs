using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;

namespace Lykke.Job.History.Workflow.Projections
{
    public class CashTransferProjection
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly ILog _logger;

        public CashTransferProjection(IHistoryRecordsRepository historyRecordsRepository, ILogFactory logFactory)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _logger = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> Handle(CashTransferProcessedEvent @event)
        {
            var cashInOuts = Mapper.Map<IEnumerable<BaseHistoryRecord>>(@event);

            foreach (var cashInOut in cashInOuts)
                if (!await _historyRecordsRepository.TryInsertAsync(cashInOut))
                    _logger.Warning($"Skipped duplicated transfer record", context: new
                    {
                        id = @event.OperationId
                    });

            return CommandHandlingResult.Ok();
        }
    }
}
