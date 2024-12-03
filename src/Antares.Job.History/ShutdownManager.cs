using System.Threading.Tasks;
using Antares.Job.History.Workflow.ExecutionProcessing;
using Antares.Sdk.Services;

namespace Antares.Job.History
{
    public class ShutdownManager : IShutdownManager
    {
        private readonly ExecutionQueueReader _executionQueueReader;

        public ShutdownManager(
            ExecutionQueueReader executionQueueReader)
        {
            _executionQueueReader = executionQueueReader;
        }

        public Task StopAsync()
        {
            _executionQueueReader.Stop();

            return Task.CompletedTask;
        }
    }
}
