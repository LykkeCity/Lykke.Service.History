using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common;
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
            int batchCount,
            IReadOnlyList<string> walletIds)
            : base(logFactory, connectionString, prefetchCount, batchCount, walletIds)
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

                    var orders = Mapper.Map<IEnumerable<Order>>(message).ToList();

                    foreach (var order in orders.Where(x => WalletIds.Contains(x.WalletId.ToString())))
                    {
                        Log.Info("Order from ME (ExecutionProcessedEvent)", context: new {orderId = order.Id, status = order.Status, order.SequenceNumber}.ToJson());
                    }

                    Queue.Enqueue(new CustomQueueItem<IEnumerable<Order>>(orders,
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
            var batchId = Guid.NewGuid().ToString();

            foreach (var order in orders.Where(x => WalletIds.Contains(x.WalletId.ToString())))
            {
                Log.Info("Saving order (ProcessBatch)", context: new {orderId = order.Id, status = order.Status, order.SequenceNumber, batchId}.ToJson());
            }

            await _ordersRepository.UpsertBulkAsync(orders);

            var trades = orders.SelectMany(x => x.Trades);

            var batched = trades.Batch(TradesBulkSize).ToList();

            foreach (var tradesBatch in batched)
                await _historyRecordsRepository.InsertBulkAsync(tradesBatch);

            TelemetryHelper.SubmitOperationResult(operation);
        }

        protected override void LogQueue()
        {
            while (Queue.Count > 0)
            {
                if (Queue.TryDequeue(out var item))
                {
                    var orders = item.Value.Select(x => new {orderId = x.Id, x.Status, x.SequenceNumber}).ToList()
                        .ToJson();

                    Log.Info("Orders in queue on shutdown", context: orders);
                }
            }
        }
    }
}
