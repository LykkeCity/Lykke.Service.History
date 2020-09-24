using System.Threading.Tasks;
using Lykke.Job.History.Workflow.ExecutionProcessing;
using Lykke.Sdk;

namespace Lykke.Job.History
{
    public class ShutdownManager : IShutdownManager
    {
        private readonly ExecutionQueueReader _executionQueueReader;
        private readonly OrderEventQueueReader _orderEventQueueReader;

        public ShutdownManager(
            ExecutionQueueReader executionQueueReader,
            OrderEventQueueReader orderEventQueueReader
            )
        {
            _executionQueueReader = executionQueueReader;
            _orderEventQueueReader = orderEventQueueReader;
        }

        public Task StopAsync()
        {
            _executionQueueReader.Stop();
            _orderEventQueueReader.Stop();

            return Task.CompletedTask;
        }
    }
}
