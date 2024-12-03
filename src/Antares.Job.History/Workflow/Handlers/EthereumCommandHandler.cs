using System;
using System.Threading.Tasks;
using Antares.Service.History.Core;
using Antares.Service.History.Core.Domain.History;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Job.EthereumCore.Contracts.Enums;
using MessagePack;

namespace Antares.Job.History.Workflow.Handlers
{
    public class EthereumCommandHandler
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly ILog _logger;

        public EthereumCommandHandler(IHistoryRecordsRepository historyRecordsRepository, ILogFactory logFactory)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _logger = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> Handle(SaveEthInHistoryCommand command)
        {
            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(command.CashinOperationId,
                command.TransactionHash))
                _logger.Warning($"Transaction hash was not set, ETH/ERC20 cashin", context: new
                {
                    id = command.CashinOperationId,
                    hash = command.TransactionHash
                });

            return CommandHandlingResult.Ok();
        }

        public async Task<CommandHandlingResult> Handle(ProcessHotWalletErc20EventCommand command)
        {
            // we need only cashout completed events here
            if (command.EventType != HotWalletEventType.CashoutCompleted)
                return CommandHandlingResult.Ok();

            if (!Utils.TryExtractGuid(command.OperationId, out var id))
            {
                _logger.Warning($"Cannot parse OperationId: {command.OperationId}");
                return CommandHandlingResult.Ok();
            }

            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(id, command.TransactionHash))
                _logger.Warning($"Transaction hash was not set, ERC20 cashout", context: new
                {
                    id,
                    hash = command.TransactionHash
                });

            return CommandHandlingResult.Ok();
        }

        public async Task<CommandHandlingResult> Handle(ProcessEthCoinEventCommand command)
        {
            // we need only cashout completed events here
            if (command.CoinEventType != CoinEventType.CashoutCompleted)
                return CommandHandlingResult.Ok();

            if (!Utils.TryExtractGuid(command.OperationId, out var id))
            {
                _logger.Warning($"Cannot parse OperationId: {command.OperationId}");
                return CommandHandlingResult.Ok();
            }

            if (!await _historyRecordsRepository.UpdateBlockchainHashAsync(id, command.TransactionHash))
                _logger.Warning($"Transaction hash was not set, ETH cashout", context: new
                {
                    id,
                    hash = command.TransactionHash
                });

            return CommandHandlingResult.Ok();
        }
    }

    [MessagePackObject(true)]
    public class SaveEthInHistoryCommand
    {
        public string TransactionHash { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
        public string ClientAddress { get; set; }
        public Guid ClientId { get; set; }
        public Guid CashinOperationId { get; set; }
    }

    [MessagePackObject(true)]
    public class ProcessEthCoinEventCommand
    {
        public string OperationId { get; set; }
        public CoinEventType CoinEventType { get; set; }
        public string TransactionHash { get; set; }
        public string ContractAddress { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string Amount { get; set; }
        public string Additional { get; set; }
        public DateTime EventTime { get; set; }
    }

    [MessagePackObject(true)]
    public class ProcessHotWalletErc20EventCommand
    {
        public string OperationId { get; set; }
        public string TransactionHash { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string Amount { get; set; }
        public DateTime EventTime { get; set; }
        public HotWalletEventType EventType { get; set; }
        public string TokenAddress { get; set; }
    }
}
