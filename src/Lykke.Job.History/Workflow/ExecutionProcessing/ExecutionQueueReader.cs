using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.PostProcessing.Contracts.Cqrs;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using MoreLinq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Lykke.Job.History.Workflow.ExecutionProcessing
{
    public class ExecutionQueueReader : BaseBatchQueueReader<IEnumerable<Order>>
    {
        private const int TradesBulkSize = 5000;

        private readonly IHistoryRecordsRepository _historyRecordsRepository;
        private readonly IOrdersRepository _ordersRepository;

        public ExecutionQueueReader(
            ILogFactory logFactory,
            string connectionString,
            IHistoryRecordsRepository historyRecordsRepository,
            IOrdersRepository ordersRepository,
            int prefetchCount,
            int batchCount)
            : base(logFactory, connectionString, prefetchCount, batchCount)
        {
            _historyRecordsRepository = historyRecordsRepository;
            _ordersRepository = ordersRepository;
        }

        protected override string ExchangeName => $"lykke.{PostProcessingBoundedContext.Name}.events.exchange";

        protected override string QueueName => $"lykke.history.queue.{PostProcessingBoundedContext.Name}.events.execution-reader";

        protected override string[] RoutingKeys => new[] { nameof(ExecutionProcessedEvent) };

        protected override EventHandler<BasicDeliverEventArgs> CreateOnMessageReceivedEventHandler(IModel channel)
        {
            var deserializer = new ProtobufMessageDeserializer<ExecutionProcessedEvent>();

            void OnMessageReceived(object o, BasicDeliverEventArgs basicDeliverEventArgs)
            {
                try
                {
                    var message = deserializer.Deserialize(basicDeliverEventArgs.Body);

                    Queue.Enqueue(new CustomQueueItem<IEnumerable<Order>>(Mapper.Map<IEnumerable<Order>>(message),
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

        protected override async Task ProcessBatch(IList<CustomQueueItem<IEnumerable<Order>>> batch)
        {
            var operation = TelemetryHelper.InitTelemetryOperation("Processing orders from ExecutionProcessedEvent", Guid.NewGuid().ToString());
            var orders = batch.SelectMany(x => x.Value).ToList();

            await _ordersRepository.UpsertBulkAsync(orders);

            var trades = orders.SelectMany(x => x.Trades);

            var batched = trades.Batch(TradesBulkSize).ToList();

            foreach (var tradesBatch in batched)
                await _historyRecordsRepository.InsertBulkAsync(tradesBatch);

            TelemetryHelper.SubmitOperationResult(operation);
        }
    }
}
