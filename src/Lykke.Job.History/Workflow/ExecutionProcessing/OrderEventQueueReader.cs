using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.PostProcessing.Contracts.Cqrs;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Lykke.Job.History.Workflow.ExecutionProcessing
{
    public class OrderEventQueueReader : BaseBatchQueueReader<OrderEvent>
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;

        public OrderEventQueueReader(
            ILogFactory logFactory,
            string connectionString,
            IHistoryRecordsRepository historyRecordsRepository,
            int prefetchCount,
            int batchCount)
            : base(logFactory, connectionString, prefetchCount, batchCount)
        {
            _historyRecordsRepository = historyRecordsRepository;
        }

        protected override string ExchangeName => $"lykke.{PostProcessingBoundedContext.Name}.events.exchange";

        protected override string QueueName => $"lykke.history.queue.{PostProcessingBoundedContext.Name}.events.order-event-reader";

        protected override string[] RoutingKeys => new[] { nameof(OrderPlacedEvent), nameof(OrderCancelledEvent) };

        protected override EventHandler<BasicDeliverEventArgs> CreateOnMessageReceivedEventHandler(IModel channel)
        {
            var orderPlacedDeserializer = new ProtobufMessageDeserializer<OrderPlacedEvent>();
            var orderCancelledDeserializer = new ProtobufMessageDeserializer<OrderCancelledEvent>();

            void OnMessageReceived(object o, BasicDeliverEventArgs basicDeliverEventArgs)
            {
                try
                {
                    switch (basicDeliverEventArgs.RoutingKey)
                    {
                        case nameof(OrderPlacedEvent):
                            Queue.Enqueue(new CustomQueueItem<OrderEvent>(
                                Mapper.Map<OrderEvent>(
                                    orderPlacedDeserializer.Deserialize(basicDeliverEventArgs.Body)),
                                basicDeliverEventArgs.DeliveryTag, channel));
                            break;
                        case nameof(OrderCancelledEvent):
                            Queue.Enqueue(new CustomQueueItem<OrderEvent>(
                                Mapper.Map<OrderEvent>(
                                    orderCancelledDeserializer.Deserialize(basicDeliverEventArgs.Body)),
                                basicDeliverEventArgs.DeliveryTag, channel));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(basicDeliverEventArgs.RoutingKey);
                    }

                }
                catch (Exception e)
                {
                    Log.Error(e);

                    channel.BasicReject(basicDeliverEventArgs.DeliveryTag, false);
                }
            }

            return OnMessageReceived;
        }

        protected override async Task ProcessBatch(IList<CustomQueueItem<OrderEvent>> batch)
        {
            var operation = TelemetryHelper.InitTelemetryOperation($"Processing OrderEvents", Guid.NewGuid().ToString());
            await _historyRecordsRepository.InsertBulkAsync(batch.Select(x => x.Value));
            TelemetryHelper.SubmitOperationResult(operation);
        }
    }
}
