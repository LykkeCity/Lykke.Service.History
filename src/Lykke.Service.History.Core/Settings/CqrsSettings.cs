using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.History.Core.Settings
{
    public class CqrsSettings
    {
        [AmqpCheck]
        public string RabbitConnString { get; set; }
    }
}
