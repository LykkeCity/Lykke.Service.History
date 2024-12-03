using System;
using Antares.Sdk;
using Antares.Service.History.AutoMapper;
using Antares.Service.History.Core.Settings;
using Antares.Service.History.GrpcServices;
using Antares.Service.History.PostgresRepositories.Mappings;
using Autofac;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Antares.Service.History
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "HistoryRecord API",
            ApiVersion = "v1"
        };

        private LykkeServiceOptions<AppSettings> _lykkeOptions;
        private IReloadingManagerWithConfiguration<AppSettings> _settings;

        [UsedImplicitly]
        public void ConfigureContainer(ContainerBuilder builder)
        {
            var configurationRoot = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            builder.ConfigureContainerBuilder(_lykkeOptions, configurationRoot, _settings);
        }

        [UsedImplicitly]
        public void ConfigureServices(IServiceCollection services)
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfiles(typeof(ServiceProfile));
                cfg.AddProfiles(typeof(RepositoryProfile));
            });

            (_lykkeOptions, _settings) = services.ConfigureServices<AppSettings>(options =>
            {
                options.SwaggerOptions = _swaggerOptions;
                
                options.Logs = logs =>
                {
                    logs.AzureTableName = "HistoryLog";
                    logs.AzureTableConnectionStringResolver = settings => settings.HistoryService.Db.LogsConnString;

                    // TODO: You could add extended logging configuration here:
                    /* 
                    logs.Extended = extendedLogs =>
                    {
                        // For example, you could add additional slack channel like this:
                        extendedLogs.AddAdditionalSlackChannel("HistoryRecord", channelOptions =>
                        {
                            channelOptions.MinLogLevel = LogLevel.Information;
                        });
                    };
                    */
                };

                // TODO: Extend the service configuration
                /*
                options.Extend = (sc, settings) =>
                {
                    sc
                        .AddOptions()
                        .AddAuthentication(MyAuthOptions.AuthenticationScheme)
                        .AddScheme<MyAuthOptions, KeyAuthHandler>(MyAuthOptions.AuthenticationScheme, null);
                };
                */

                // TODO: You could add extended Swagger configuration here:
                /*
                options.Swagger = swagger =>
                {
                    swagger.IgnoreObsoleteActions();
                };
                */
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;
                options.RegisterEndpoints = builder =>
                {
                    builder.MapGrpcService<IsAliveService>();
                    builder.MapGrpcService<HistoryService>();
                    builder.MapGrpcService<OrderService>();
                    builder.MapGrpcService<TradesService>();
                };
                // TODO: Configure additional middleware for eg authentication or maintenancemode checks
                /*
                options.WithMiddleware = x =>
                {
                    x.UseMaintenanceMode<AppSettings>(settings => new MaintenanceMode
                    {
                        Enabled = settings.MaintenanceMode?.Enabled ?? false,
                        Reason = settings.MaintenanceMode?.Reason
                    });
                    x.UseAuthentication();
                };
                */
            });
        }
    }
}
