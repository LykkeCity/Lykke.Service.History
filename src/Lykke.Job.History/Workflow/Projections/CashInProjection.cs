using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;

namespace Lykke.Job.History.Workflow.Projections
{
    public class CashInProjection
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly ILog _logger;

        public CashInProjection(IHistoryRecordsRepository historyRecordsRepository, ILogFactory logFactory)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _logger = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> Handle(CashInProcessedEvent @event)
        {
            var entity = Mapper.Map<Cashin>(@event);

            if (!await _historyRecordsRepository.TryInsertAsync(entity))
                _logger.Warning($"Skipped duplicated cashin record", context: new
                {
                    id = @event.OperationId
                });

            return CommandHandlingResult.Ok();
        }
    }
}
