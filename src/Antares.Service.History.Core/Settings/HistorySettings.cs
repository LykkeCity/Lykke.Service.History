using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Antares.Service.History.Core.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class HistorySettings
    {
        public DbSettings Db { get; set; }

        public CqrsSettings Cqrs { get; set; }

        public RabbitMqSettings MatchingEngineRabbit
        {
            get;
            set;
        }

        [Optional]
        public int RabbitPrefetchCount { get; set; } = 500;

        [Optional]
        public int PostgresOrdersBatchSize { get; set; } = 100;

        [Optional]
        public IReadOnlyList<string> WalletIdsToLog { get; set; } = Array.Empty<string>();

        [Optional] 
        public bool CqrsEnabled { get; set; } = false;
    }
}
