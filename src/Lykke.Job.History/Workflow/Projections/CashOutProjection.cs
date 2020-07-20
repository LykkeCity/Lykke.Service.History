using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;

namespace Lykke.Job.History.Workflow.Projections
{
    public class CashOutProjection
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly ILog _logger;

        public CashOutProjection(IHistoryRecordsRepository historyRecordsRepository, ILogFactory logFactory)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _logger = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> Handle(CashOutProcessedEvent @event)
        {
            var entity = Mapper.Map<Cashout>(@event);

            if (!await _historyRecordsRepository.TryInsertAsync(entity))
                _logger.Warning($"Skipped duplicated cashout record", context: new
                {
                    id = @event.OperationId
                });

            return CommandHandlingResult.Ok();
        }
    }
}
