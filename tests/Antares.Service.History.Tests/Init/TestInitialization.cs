using System;
using System.Collections.Generic;
using Antares.Job.History.Workflow.Handlers;
using Antares.Job.History.Workflow.Projections;
using Antares.Service.History.Contracts.Cqrs;
using Antares.Service.History.Contracts.Cqrs.Commands;
using Antares.Service.History.Contracts.Cqrs.Events;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.Core.Domain.Orders;
using Antares.Service.History.Core.Settings;
using Antares.Service.History.Modules;
using Antares.Service.History.PostgresRepositories.Mappings;
using Autofac;
using AutoMapper;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.SettingsReader.ReloadingManager;
using Xunit;

namespace Antares.Service.History.Tests.Init
{
    [CollectionDefinition("history-tests")]
    public class TestInitializationCollection : ICollectionFixture<TestInitialization>
    {
    }

    public class TestInitialization
    {
        static TestInitialization()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfiles(typeof(Antares.Job.History.AutoMapper.ServiceProfile));
                cfg.AddProfiles(typeof(RepositoryProfile));
            });
        }

        public TestInitialization()
        {
            Container = CreateContainer();
        }

        public IContainer Container { get; }

        public IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(LogFactory.Create().AddUnbufferedConsole()).As<ILogFactory>().SingleInstance();

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>()
                .SingleInstance();

            builder.RegisterType<ForwardWithdrawalCommandHandler>();

            builder.Register(ctx =>
                {
                    var logFactory = ctx.Resolve<ILogFactory>();
                    var messagingEngine = new MessagingEngine(
                        logFactory,
                        new TransportResolver(new Dictionary<string, TransportInfo>
                        {
                            {"InMemory", new TransportInfo("none", "none", "none", null, "InMemory")}
                        }));
                    var cqrsEngine = CreateEngine(ctx, messagingEngine, logFactory);
                    cqrsEngine.StartAll();
                    return cqrsEngine;
                })
                .As<ICqrsEngine>()
                .AutoActivate()
                .SingleInstance();

            RegisterRepo(builder);

            return builder.Build();
        }

        private void RegisterRepo(ContainerBuilder builder)
        {
            var connection = Environment.GetEnvironmentVariable("PostgresConnection");

            if (string.IsNullOrWhiteSpace(connection))
            {
                builder.RegisterType<InMemoryHistoryRepository>()
                    .As<IHistoryRecordsRepository>()
                    .SingleInstance();

                builder.RegisterType<InMemoryOrdersRepository>()
                    .As<IOrdersRepository>()
                    .SingleInstance();
            }
            else
            {
                var settings = new AppSettings
                {
                    HistoryService = new HistorySettings
                    {
                        Db = new DbSettings
                        {
                            PostgresDataConnString = connection
                        }
                    }
                };

                builder.RegisterModule(new DbModule(ConstantReloadingManager.From(settings)));
            }
        }

        private CqrsEngine CreateEngine(
            IComponentContext ctx,
            IMessagingEngine messagingEngine,
            ILogFactory logFactory)
        {
            const string defaultRoute = "self";

            return new CqrsEngine(
                logFactory,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,
                Register.DefaultEndpointResolver(new InMemoryEndpointResolver()),

                Register.BoundedContext(HistoryBoundedContext.Name)
                    .ListeningCommands(typeof(CreateForwardCashinCommand), typeof(DeleteForwardCashinCommand))
                    .On(defaultRoute)
                    .WithLoopback()
                    .WithCommandsHandler<ForwardWithdrawalCommandHandler>()
                    .PublishingEvents(typeof(ForwardCashinCreatedEvent), typeof(ForwardCashinDeletedEvent))
                    .With("events"));

        }
    }
}
