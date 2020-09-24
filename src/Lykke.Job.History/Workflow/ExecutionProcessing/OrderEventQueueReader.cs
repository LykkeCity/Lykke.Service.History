using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common;
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
            int batchCount,
            IReadOnlyList<string> walletIds)
            : base(logFactory, connectionString, prefetchCount, batchCount, walletIds)
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

        protected override Task ProcessBatch(IList<CustomQueueItem<OrderEvent>> batch)
        {
            return _historyRecordsRepository.InsertBulkAsync(batch.Select(x => x.Value));
        }

        protected override void LogQueue()
        {
            while (Queue.Count > 0)
            {
                if (Queue.TryDequeue(out var item))
                {
                    var order = new {orderId = item.Value.Id, item.Value.Status}.ToJson();

                    Log.Info("Orders in queue on shutdown", context: order);
                }
            }
        }
    }
}
