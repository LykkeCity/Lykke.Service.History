using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Antares.Service.History.Core.Settings
{
    [UsedImplicitly]
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
        [AmqpCheck]
        [Optional]
        public string AlternativeConnectionString { get; set; }
        public string Exchange { get; set; }
    }
}
