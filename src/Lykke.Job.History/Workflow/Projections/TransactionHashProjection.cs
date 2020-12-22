using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bitcoin.Contracts.Events;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Service.History.Core.Domain.History;

namespace Lykke.Job.History.Workflow.Projections
{
    public class TransactionHashProjection
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly ILog _logger;
        private readonly string _crossClientTransactionHashSubstituition = "0x";

        public TransactionHashProjection(IHistoryRecordsRepository historyRecordsRepository, ILogFactory logFactory)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _logger = logFactory.CreateLog(this);
        }

        /// <summary>
        ///     Bitcoin cashin event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(CashinCompletedEvent @event)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(@event.OperationId, @event.TxHash))
            {
                _logger.Warning($"Transaction hash was not set", context: new
                {
                    id = @event.OperationId,
                    hash = @event.TxHash
                });

                return CommandHandlingResult.Fail(TimeSpan.FromMinutes(1));
            }

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        ///     Bitcoin cashout event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(CashoutCompletedEvent @event)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(@event.OperationId, @event.TxHash))
            {
                _logger.Warning($"Transaction hash was not set", context: new
                {
                    id = @event.OperationId,
                    hash = @event.TxHash
                });

                return CommandHandlingResult.Fail(TimeSpan.FromMinutes(1));
            }

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        ///     BIL cashin event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(
            Job.BlockchainCashinDetector.Contract.Events.CashinCompletedEvent @event)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(@event.OperationId, @event.TransactionHash))
            {
                _logger.Warning($"Transaction hash was not set", context: new
                {
                    id = @event.OperationId,
                    hash = @event.TransactionHash
                });

                return CommandHandlingResult.Fail(TimeSpan.FromMinutes(1));
            }

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        ///     Sirius cashin event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(
            Job.SiriusDepositsDetector.Contract.Events.CashinCompletedEvent @event)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(@event.OperationId, @event.TransactionHash))
            {
                _logger.Warning($"Transaction hash was not set", context: new
                {
                    id = @event.OperationId,
                    hash = @event.TransactionHash
                });

                return CommandHandlingResult.Fail(TimeSpan.FromMinutes(1));
            }

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        ///     BIL cashout event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(
            Job.BlockchainCashoutProcessor.Contract.Events.CashoutCompletedEvent @event)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(@event.OperationId, @event.TransactionHash))
            {
                _logger.Warning($"Transaction hash was not set", context: new
                {
                    id = @event.OperationId,
                    hash = @event.TransactionHash
                });

                return CommandHandlingResult.Fail(TimeSpan.FromMinutes(1));
            }

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        ///     Sirius cashout event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(
            Job.SiriusCashoutProcessor.Contract.Events.CashoutCompletedEvent @event)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(@event.OperationId, @event.TransactionHash))
            {
                _logger.Warning($"Transaction hash was not set", context: new
                {
                    id = @event.OperationId,
                    hash = @event.TransactionHash
                });

                return CommandHandlingResult.Fail(TimeSpan.FromMinutes(1));
            }

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        ///     BIL batched cashout event
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(
            Job.BlockchainCashoutProcessor.Contract.Events.CashoutsBatchCompletedEvent @event)
        {
            if (@event.Cashouts == null || @event.Cashouts.Length == 0)
            {
                _logger.Warning($"BIL batched cashout event is empty, BatchId {@event.BatchId}", context: @event);

                return CommandHandlingResult.Ok();
            }

            foreach (var cashout in @event.Cashouts)
            {
                if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(cashout.OperationId,
                    @event.TransactionHash))
                {
                    _logger.Warning($"Transaction hash was not set, BIL cashout", context: new
                    {
                        id = cashout.OperationId,
                        hash = @event.TransactionHash
                    });

                    return CommandHandlingResult.Fail(TimeSpan.FromMinutes(1));
                }
            }

            return CommandHandlingResult.Ok();
        }

        /// <summary>
        ///     BIL cross client cashout event completed
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<CommandHandlingResult> Handle(
            Job.BlockchainCashoutProcessor.Contract.Events.CrossClientCashoutCompletedEvent @event)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(@event.OperationId,
                _crossClientTransactionHashSubstituition))
            {
                _logger.Warning($"Transaction hash was not set. " +
                                $"OperationId: {@event.OperationId}, " +
                                $"TxHash: {_crossClientTransactionHashSubstituition}", context: new
                                {
                                    id = @event.OperationId,
                                    hash = _crossClientTransactionHashSubstituition
                                });

                return CommandHandlingResult.Fail(TimeSpan.FromMinutes(1));
            }

            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(@event.CashinOperationId,
                _crossClientTransactionHashSubstituition))
            {
                _logger.Warning($"Transaction cashin hash was not set. " +
                                $"OperationId: {@event.OperationId}, " +
                                $"TxHash: {_crossClientTransactionHashSubstituition}", context: new
                {
                    id = @event.OperationId,
                    hash = _crossClientTransactionHashSubstituition
                });

                return CommandHandlingResult.Fail(TimeSpan.FromMinutes(1));
            }

            return CommandHandlingResult.Ok();
        }
    }
}
