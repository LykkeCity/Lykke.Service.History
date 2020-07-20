using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.History.Core.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public HistorySettings HistoryService { get; set; }
    }
}
