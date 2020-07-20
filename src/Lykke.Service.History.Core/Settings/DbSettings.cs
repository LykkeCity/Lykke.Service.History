using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.History.Core.Settings
{
    public class DbSettings
    {
        public string PostgresDataConnString { get; set; }

        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
