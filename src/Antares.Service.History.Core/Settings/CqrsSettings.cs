using Lykke.SettingsReader.Attributes;

namespace Antares.Service.History.Core.Settings
{
    public class CqrsSettings
    {
        [AmqpCheck]
        public string RabbitConnString { get; set; }
    }
}
