using Autofac;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.History.Core.Settings;
using Lykke.Service.History.PostgresRepositories;
using Lykke.Service.History.PostgresRepositories.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Job.History.Modules
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
