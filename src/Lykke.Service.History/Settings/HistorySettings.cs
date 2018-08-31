﻿using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.History.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class HistorySettings
    {
        public DbSettings Db { get; set; }

        public CqrsSettings Cqrs { get; set; }

        [Optional]
        public int RabbitPrefetchCount { get; set; } = 500;

        [Optional]
        public int PostgresOrdersBatchSize { get; set; } = 100;
    }
}
