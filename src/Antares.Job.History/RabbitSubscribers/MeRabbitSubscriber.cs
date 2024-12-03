using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antares.Service.History.Core.Domain.Enums;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.Core.Settings;
using Autofac;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.MatchingEngine.Connector.Models.Events;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Deduplication;
using Lykke.RabbitMqBroker.Subscriber;
using Utils = Antares.Service.History.Core.Utils;

namespace Antares.Job.History.RabbitSubscribers
{
    [UsedImplicitly]
    public class MeRabbitSubscriber : IStartable, IStopable
    {
        [NotNull] private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _rabbitMqSettings;
        private readonly List<IStopable> _subscribers = new List<IStopable>();
        private readonly IDeduplicator _deduplicator;
        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly ILog _log;

        private const string QueueName = "lykke.spot.matching.engine.out.events.antares-history";
        private const bool QueueDurable = true;

        public MeRabbitSubscriber(
            [NotNull] ILogFactory logFactory,
            [NotNull] RabbitMqSettings rabbitMqSettings,
            [NotNull] IDeduplicator deduplicator,
            IHistoryRecordsRepository historyRecordsRepository)
        {
            _logFactory = logFactory ?? throw new ArgumentNullException(nameof(logFactory));
            _log = _logFactory.CreateLog(this);
            _rabbitMqSettings = rabbitMqSettings ?? throw new ArgumentNullException(nameof(rabbitMqSettings));
            _deduplicator = deduplicator ?? throw new ArgumentNullException(nameof(deduplicator));
            _historyRecordsRepository = historyRecordsRepository;
        }

        public void Start()
        {
            _subscribers.Add(Subscribe<CashInEvent>(Lykke.MatchingEngine.Connector.Models.Events.Common.MessageType.CashIn, ProcessMessageAsync));
            _subscribers.Add(Subscribe<CashOutEvent>(Lykke.MatchingEngine.Connector.Models.Events.Common.MessageType.CashOut, ProcessMessageAsync));
            _subscribers.Add(Subscribe<CashTransferEvent>(Lykke.MatchingEngine.Connector.Models.Events.Common.MessageType.CashTransfer, ProcessMessageAsync));
        }

        private RabbitMqSubscriber<T> Subscribe<T>(Lykke.MatchingEngine.Connector.Models.Events.Common.MessageType messageType, Func<T, Task> func)
        {
            var settings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _rabbitMqSettings.ConnectionString,
                QueueName = $"{QueueName}.{messageType}",
                ExchangeName = _rabbitMqSettings.Exchange,
                RoutingKey = ((int)messageType).ToString(),
                IsDurable = QueueDurable
            };

            return new RabbitMqSubscriber<T>(
                    _logFactory,
                    settings,
                    new ResilientErrorHandlingStrategy(_logFactory, settings,
                        retryTimeout: TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_logFactory, settings)))
                .SetMessageDeserializer(new ProtobufMessageDeserializer<T>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(func)
                .CreateDefaultBinding()
                .SetAlternativeExchange(_rabbitMqSettings.AlternativeConnectionString)
                .SetDeduplicator(_deduplicator)
                .Start();
        }


        private async Task ProcessMessageAsync(CashInEvent message)
        {
            var fees = message.CashIn.Fees;
            var fee = fees?.FirstOrDefault()?.Transfer;

            var entity = new Cashin()
            {
                AssetId = message.CashIn.AssetId,
                //BlockchainHash = ,
                FeeSize = ParseNullabe(fee?.Volume),
                Id = Guid.Parse(message.Header.RequestId),
                State = HistoryState.Finished,
                Timestamp = message.Header.Timestamp,
                Volume = Math.Abs(decimal.Parse(message.CashIn.Volume)),
                WalletId = Guid.Parse(message.CashIn.WalletId)
            };

            if (!await _historyRecordsRepository.TryInsertAsync(entity))
                _log.Warning($"Skipped duplicated cashin record", context: new
                {
                    id = message.Header.RequestId
                });
        }

        private async Task ProcessMessageAsync(CashOutEvent message)
        {
            var fees = message.CashOut.Fees;
            var fee = fees?.FirstOrDefault()?.Transfer;

            var entity = new Cashout()
            {
                //BlockchainHash = ,
                FeeSize = ParseNullabe(fee?.Volume),
                State = HistoryState.Finished,
                Id = Guid.Parse(message.Header.RequestId),
                WalletId = Guid.Parse(message.CashOut.WalletId),
                AssetId = message.CashOut.AssetId,
                Timestamp = message.Header.Timestamp,
                Volume = -Math.Abs(decimal.Parse(message.CashOut.Volume))
            };

            if (!await _historyRecordsRepository.TryInsertAsync(entity))
                _log.Warning($"Skipped duplicated cashout record", context: new
                {
                    id = message.Header.RequestId
                });
        }

        private async Task ProcessMessageAsync(CashTransferEvent message)
        {
            var fees = message.CashTransfer.Fees;
            var fee = fees?.FirstOrDefault()?.Transfer;
            var operationId = Guid.Parse(message.Header.RequestId);
            var fromWallet = Guid.Parse(message.CashTransfer.FromWalletId);
            var toWallet = Guid.Parse(message.CashTransfer.ToWalletId);
            var volume = decimal.Parse(message.CashTransfer.Volume);
            var timestamp = message.Header.Timestamp;
            var assetId = message.CashTransfer.AssetId;
            var feeSourceWalletId = fee != null ? Guid.Parse(fee.SourceWalletId) : (Guid?)null;
            var feeSize = ParseNullabe(fee?.Volume);

            var cashInOuts = new BaseHistoryRecord[] {
                new Cashout()
                {
                    Id = operationId,
                    WalletId = fromWallet,
                    Volume = -Math.Abs(volume),
                    Timestamp = timestamp,
                    AssetId = assetId,
                    FeeSize = fromWallet == feeSourceWalletId ? feeSize : null,
                    State = Antares.Service.History.Core.Domain.Enums.HistoryState.Finished
                },
                new Cashin()
                {
                    Id = operationId,
                    WalletId = toWallet,
                    Volume = Math.Abs(volume),
                    Timestamp = timestamp,
                    AssetId = assetId,
                    FeeSize = toWallet == feeSourceWalletId ? feeSize : null,
                    State = Antares.Service.History.Core.Domain.Enums.HistoryState.Finished
                }};

            foreach (var cashInOut in cashInOuts)
                if (!await _historyRecordsRepository.TryInsertAsync(cashInOut))
                    _log.Warning($"Skipped duplicated transfer record", context: new
                    {
                        id = operationId
                    });
        }

        private decimal? ParseNullabe(string value)
        {
            return !string.IsNullOrEmpty(value) ? decimal.Parse(value) : (decimal?)null;
        }

        public void Dispose()
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber?.Dispose();
            }

        }

        public void Stop()
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber?.Stop();
            }
        }
    }
}
