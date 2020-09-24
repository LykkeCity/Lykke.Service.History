using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Lykke.Job.History.Workflow.ExecutionProcessing
{
    public abstract class BaseBatchQueueReader<T>
    {
        private readonly TimeSpan _reconnectTimeoutSeconds = TimeSpan.FromSeconds(30);
        private readonly TimeSpan _timeoutBeforeStop = TimeSpan.FromSeconds(10);

        private readonly string _connectionString;
        private readonly int _prefetchCount;
        private readonly int _batchCount;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _queueReaderTask;

        protected readonly ILog Log;
        protected readonly IReadOnlyList<string> WalletIds;
        protected ConcurrentQueue<CustomQueueItem<T>> Queue;

        protected BaseBatchQueueReader(
            ILogFactory logFactory,
            string connectionString,
            int prefetchCount,
            int batchCount,
            IReadOnlyList<string> walletIds)
        {
            _connectionString = connectionString;
            _prefetchCount = prefetchCount;
            _batchCount = batchCount;
            Log = logFactory.CreateLog(this);
            WalletIds = walletIds;
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Queue = new ConcurrentQueue<CustomQueueItem<T>>();
            _queueReaderTask = StartRabbitSessionWithReconnects();
        }

        public void Stop()
        {
            Log.Info("Stopping  queue reader");
            _cancellationTokenSource.Cancel();

            int index = Task.WaitAny(_queueReaderTask, Task.Delay(_timeoutBeforeStop));

            Log.Info(index == 0 ? "Queue reader stopped" : $"Unable to stop queue reader within {_timeoutBeforeStop.TotalSeconds} seconds.");
        }

        protected abstract string ExchangeName { get; }

        protected abstract string QueueName { get; }

        protected abstract string[] RoutingKeys { get; }

        protected abstract EventHandler<BasicDeliverEventArgs> CreateOnMessageReceivedEventHandler(IModel channel);

        protected abstract Task ProcessBatch(IList<CustomQueueItem<T>> batch);
        protected abstract void LogQueue();

        private async Task StartRabbitSessionWithReconnects()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    await StartQueueReader();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
                finally
                {
                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        Log.Info($"Connection will be reconnected in {_reconnectTimeoutSeconds} seconds");
                        await Task.Delay(_reconnectTimeoutSeconds);
                    }
                }
            }

            Log.Info("Session was closed");
        }

        private async Task StartQueueReader()
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_connectionString)
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.BasicQos(0, (ushort)_prefetchCount, false);

                channel.QueueDeclare(QueueName, true, false, false);

                foreach (var key in RoutingKeys)
                    channel.QueueBind(QueueName, ExchangeName, key);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += CreateOnMessageReceivedEventHandler(channel);

                var tag = channel.BasicConsume(QueueName, false, consumer);

                var writerTask = StartDbWriter(connection);

                while (!_cancellationTokenSource.IsCancellationRequested && connection.IsOpen)
                    await Task.Delay(100);

                await writerTask;

                channel.BasicCancel(tag);
                connection.Close();

                if (Queue.Count > 0)
                {
                    Log.Warning("Queue is not empty on shutdown!");
                    LogQueue();
                }

                Queue.Clear();
            }
        }

        private async Task StartDbWriter(IConnection connection)
        {
            while ((!_cancellationTokenSource.IsCancellationRequested || Queue.Count > 0) && connection.IsOpen)
            {
                var isFullBatch = false;
                try
                {
                    var exceptionThrowed = false;
                    var list = new List<CustomQueueItem<T>>();
                    try
                    {
                        for (var i = 0; i < _batchCount; i++)
                            if (Queue.TryDequeue(out var customQueueItem))
                                list.Add(customQueueItem);
                            else
                                break;

                        if (list.Count > 0)
                        {
                            isFullBatch = list.Count == _batchCount;

                            await ProcessBatch(list);
                        }
                    }
                    catch (Exception e)
                    {
                        exceptionThrowed = true;

                        Log.Error(e, "Error processing batch");

                        foreach (var item in list)
                            item.Reject();
                    }
                    finally
                    {
                        if (!exceptionThrowed)
                            foreach (var item in list)
                                item.Accept();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
                finally
                {
                    await Task.Delay(isFullBatch ? 1 : 1000);
                }
            }
        }
    }
}
