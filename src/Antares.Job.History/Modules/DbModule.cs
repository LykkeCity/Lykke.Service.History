using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.Core.Domain.Orders;
using Antares.Service.History.Core.Settings;
using Antares.Service.History.PostgresRepositories;
using Antares.Service.History.PostgresRepositories.Repositories;
using Autofac;
using Lykke.SettingsReader;

namespace Antares.Job.History.Modules
{
    public class DbModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public DbModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(
                new ConnectionFactory(_appSettings.CurrentValue.HistoryService.Db.PostgresDataConnString));

            builder.RegisterType<HistoryRecordsRepository>().As<IHistoryRecordsRepository>();

            builder.RegisterType<OrdersRepository>().As<IOrdersRepository>();
        }
    }
}
