using Antares.Sdk.Settings;
using JetBrains.Annotations;

namespace Antares.Service.History.Core.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public HistorySettings HistoryService { get; set; }
    }
}
