using Lykke.SettingsReader.Attributes;

namespace Antares.Service.History.Core.Settings
{
    public class DbSettings
    {
        public string PostgresDataConnString { get; set; }

        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
