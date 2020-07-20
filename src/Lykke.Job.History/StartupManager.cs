using System.Threading.Tasks;
using Lykke.Cqrs;
using Lykke.Job.History.Workflow.ExecutionProcessing;
using Lykke.Sdk;

namespace Lykke.Job.History
{
    public class StartupManager : IStartupManager
    {
        private readonly ICqrsEngine _cqrsEngine;
        private readonly ExecutionQueueReader _executionQueueReader;
        private readonly OrderEventQueueReader _orderEventQueueReader;

        public StartupManager(
            ICqrsEngine cqrsEngine,
            ExecutionQueueReader executionQueueReader,
            OrderEventQueueReader orderEventQueueReader
            )
        {
            _cqrsEngine = cqrsEngine;
            _executionQueueReader = executionQueueReader;
            _orderEventQueueReader = orderEventQueueReader;
        }

        public Task StartAsync()
        {
            _executionQueueReader.Start();
            _orderEventQueueReader.Start();
            _cqrsEngine.StartSubscribers();

            return Task.CompletedTask;
        }
    }
}
