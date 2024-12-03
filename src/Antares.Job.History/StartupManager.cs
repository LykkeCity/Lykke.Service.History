using System;
using System.Threading.Tasks;
using Antares.Job.History.Workflow.ExecutionProcessing;
using Antares.Sdk.Services;
using Antares.Service.History.Core.Settings;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;

namespace Antares.Job.History
{
    public class StartupManager : IStartupManager
    {
        private readonly HistorySettings _historySettings;
        private readonly ICqrsEngine _cqrsEngine;
        private readonly ExecutionQueueReader _executionQueueReader;
        private readonly ILog _log;

        public StartupManager(
            HistorySettings historySettings,
            ICqrsEngine cqrsEngine,
            ExecutionQueueReader executionQueueReader,
            ILogFactory logFactory)
        {
            _historySettings = historySettings;
            _cqrsEngine = cqrsEngine;
            _executionQueueReader = executionQueueReader;
            _log = logFactory.CreateLog(nameof(StartupManager));
        }

        public Task StartAsync()
        {
            _executionQueueReader.Start();

            if (_historySettings.CqrsEnabled)
            {
                _cqrsEngine.StartSubscribers();
            }
            else
            {
                _log.Info("CQRS Engine is disabled");
            }
                

            return Task.CompletedTask;
        }
    }
}
