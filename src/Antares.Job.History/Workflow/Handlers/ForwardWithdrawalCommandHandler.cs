using System.Threading.Tasks;
using Antares.Service.History.Contracts.Cqrs.Commands;
using Antares.Service.History.Contracts.Cqrs.Events;
using Antares.Service.History.Core.Domain.History;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;

namespace Antares.Job.History.Workflow.Handlers
{
    public class ForwardWithdrawalCommandHandler
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly ILog _logger;

        public ForwardWithdrawalCommandHandler(IHistoryRecordsRepository historyRecordsRepository, ILogFactory logFactory)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _logger = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> Handle(CreateForwardCashinCommand command, IEventPublisher eventPublisher)
        {
            var entity = Mapper.Map<Cashin>(command);

            if (!await _historyRecordsRepository.TryInsertAsync(entity))
            {
                _logger.Warning($"Skipped duplicated forward cashin record", context: new
                {
                    id = command.OperationId
                });

                return CommandHandlingResult.Ok();
            }

            eventPublisher.PublishEvent(new ForwardCashinCreatedEvent
            {
                AssetId = command.AssetId,
                OperationId = command.OperationId,
                Timestamp = command.Timestamp,
                Volume = command.Volume,
                WalletId = command.WalletId
            });

            return CommandHandlingResult.Ok();
        }

        public async Task<CommandHandlingResult> Handle(DeleteForwardCashinCommand command, IEventPublisher eventPublisher)
        {
            if (!await _historyRecordsRepository.TryDeleteAsync(command.OperationId, command.WalletId))
            {
                _logger.Warning($"Forward cashin record was not found", context: new
                {
                    id = command.OperationId
                });

                return CommandHandlingResult.Ok();
            }

            eventPublisher.PublishEvent(new ForwardCashinDeletedEvent
            {
                OperationId = command.OperationId,
                WalletId = command.WalletId
            });

            return CommandHandlingResult.Ok();
        }
    }
}
