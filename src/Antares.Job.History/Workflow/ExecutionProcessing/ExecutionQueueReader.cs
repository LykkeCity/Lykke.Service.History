using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antares.Service.History.Core.Settings;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.MatchingEngine.Connector.Models.Events;
using Lykke.RabbitMqBroker.Subscriber;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Antares.Job.History.Workflow.ExecutionProcessing
{
    public class ExecutionQueueReader : BaseBatchQueueReader<ExecutionEvent>
    {
        private readonly IReadOnlyList<string> _walletIds;
        private readonly RabbitMqSettings _rabbitMqSettings;
        private readonly ExecutionEventHandler _executionEventHandler;
        private readonly ILog _log;

        public ExecutionQueueReader(
            ILogFactory logFactory,
            int prefetchCount,
            int batchCount,
            IReadOnlyList<string> walletIds,
            RabbitMqSettings rabbitMqSettings,
            ExecutionEventHandler executionEventHandler)
            : base(logFactory, rabbitMqSettings.ConnectionString, prefetchCount, batchCount, walletIds)
        {
            _walletIds = walletIds;
            _rabbitMqSettings = rabbitMqSettings;
            _executionEventHandler = executionEventHandler;
            _log = logFactory.CreateLog(this);
        }

        protected override string ExchangeName => _rabbitMqSettings.Exchange;

        protected override string QueueName =>
            $"lykke.spot.matching.engine.out.events.antares-history.{Lykke.MatchingEngine.Connector.Models.Events.Common.MessageType.Order}";

        protected override string[] RoutingKeys => new[] { ((int)Lykke.MatchingEngine.Connector.Models.Events.Common.MessageType.Order).ToString() };

        protected override EventHandler<BasicDeliverEventArgs> CreateOnMessageReceivedEventHandler(IModel channel)
        {
            var deserializer = new ProtobufMessageDeserializer<ExecutionEvent>();

            void OnMessageReceived(object o, BasicDeliverEventArgs basicDeliverEventArgs)
            {
                try
                {
                    var message = deserializer.Deserialize(basicDeliverEventArgs.Body);

                    if (message.Orders.Any(x => _walletIds.Contains(x.WalletId.ToString())))
                    {
                        _log.Info("Adding orders in queue for processing", context: $"sequence number: {message.Header.SequenceNumber}");
                    }

                    Queue.Enqueue(new CustomQueueItem<ExecutionEvent>(message,
                        basicDeliverEventArgs.DeliveryTag, channel));
                }
                catch (Exception e)
                {
                    Log.Error(e);

                    channel.BasicReject(basicDeliverEventArgs.DeliveryTag, false);
                }
            }

            return OnMessageReceived;
        }

        protected override async Task ProcessBatch(IList<CustomQueueItem<ExecutionEvent>> batch)
        {
            await _executionEventHandler.HandleAsync(batch.Select(x => x.Value).ToArray());
        }

        protected override void LogQueue()
        {
            while (Queue.Count > 0)
            {
                if (Queue.TryDequeue(out var item))
                {
                    var orders = item.Value.Orders.Select(x => new {x.Id, x.Status, item.Value.Header.SequenceNumber}).ToList()
                        .ToJson();

                    Log.Info("Orders in queue on shutdown", context: $"orders: {orders}");
                }
            }
        }
    }
}
