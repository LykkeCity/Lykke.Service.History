using Lykke.SettingsReader.Attributes;

namespace Antares.Service.History.Client
{
    /// <summary>
    ///     History client settings.
    /// </summary>
    public class HistoryServiceClientSettings
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
