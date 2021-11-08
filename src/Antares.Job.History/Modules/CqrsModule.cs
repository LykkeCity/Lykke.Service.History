using System;
using System.Collections.Generic;
using Antares.Job.History.RabbitSubscribers;
using Antares.Job.History.Workflow.ExecutionProcessing;
using Antares.Job.History.Workflow.Handlers;
using Antares.Job.History.Workflow.Projections;
using Antares.Sdk.Services;
using Antares.Service.History.Contracts.Cqrs;
using Antares.Service.History.Contracts.Cqrs.Commands;
using Antares.Service.History.Contracts.Cqrs.Events;
using Antares.Service.History.Core.Settings;
using Autofac;
using Lykke.Bitcoin.Contracts;
using Lykke.Bitcoin.Contracts.Events;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Cqrs.Middleware.Logging;
using Lykke.Job.BlockchainCashinDetector.Contract;
using Lykke.Job.BlockchainCashoutProcessor.Contract;
using Lykke.Job.SiriusCashoutProcessor.Contract;
using Lykke.Job.SiriusDepositsDetector.Contract;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.RabbitMqBroker.Deduplication;
using Lykke.SettingsReader;
using RabbitMQ.Client;

namespace Antares.Job.History.Modules
{
    public class CqrsModule : Module
    {
        private readonly HistorySettings _settings;

        public CqrsModule(IReloadingManager<AppSettings> settingsManager)
        {
            _settings = settingsManager.CurrentValue.HistoryService;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings).AsSelf();

            builder.RegisterType<InMemoryDeduplcator>()
                .As<IDeduplicator>()
                .SingleInstance();

            builder
                .RegisterType<ExecutionEventHandler>()
                .WithParameter(TypedParameter.From(_settings.MatchingEngineRabbit))
                .WithParameter(TypedParameter.From(_settings.WalletIdsToLog))
                .SingleInstance();

            builder.RegisterType<MeRabbitSubscriber>()
                .As<IStartable>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.MatchingEngineRabbit));

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>()
                .SingleInstance();

            var rabbitMqSettings = new ConnectionFactory
            {
                Uri = new Uri(_settings.Cqrs.RabbitConnString)
            };
            var rabbitMqEndpoint = rabbitMqSettings.Endpoint.ToString();

            builder.RegisterType<StartupManager>().As<IStartupManager>();
            builder.RegisterType<ShutdownManager>().As<IShutdownManager>();

            builder.RegisterType<ExecutionQueueReader>()
                .WithParameter(TypedParameter.From(_settings.WalletIdsToLog))
                .WithParameter(TypedParameter.From(_settings.MatchingEngineRabbit))
                .WithParameter(new NamedParameter("prefetchCount", _settings.RabbitPrefetchCount))
                .WithParameter(new NamedParameter("batchCount", _settings.PostgresOrdersBatchSize))
                .SingleInstance();

            builder.RegisterType<TransactionHashProjection>();

            builder.RegisterType<EthereumCommandHandler>();
            builder.RegisterType<ForwardWithdrawalCommandHandler>();

            builder.Register(ctx =>
                {
                    var logFactory = ctx.Resolve<ILogFactory>();
                    var messagingEngine = new MessagingEngine(
                        logFactory,
                        new TransportResolver(
                            new Dictionary<string, TransportInfo>
                            {
                                {
                                    "RabbitMq",
                                    new TransportInfo(
                                        rabbitMqEndpoint,
                                        rabbitMqSettings.UserName,
                                        rabbitMqSettings.Password, "None", "RabbitMq")
                                }
                            }),
                        new RabbitMqTransportFactory(logFactory));
                    var cqrsEngine = CreateEngine(ctx, messagingEngine, logFactory);

                    if (_settings.CqrsEnabled)
                        cqrsEngine.StartPublishers();

                    return cqrsEngine;
                })
                .As<ICqrsEngine>()
                .SingleInstance();
        }

        private CqrsEngine CreateEngine(
            IComponentContext ctx,
            IMessagingEngine messagingEngine,
            ILogFactory logFactory)
        {
            const string defaultRoute = "self";

            var sagasMessagePackEndpointResolver = new RabbitMqConventionEndpointResolver(
                "RabbitMq",
                SerializationFormat.MessagePack,
                environment: "lykke");

            var sagasProtobufEndpointResolver = new RabbitMqConventionEndpointResolver(
                "RabbitMq",
                SerializationFormat.ProtoBuf,
                environment: "lykke");

            return new CqrsEngine(
                logFactory,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,
                Register.DefaultEndpointResolver(sagasProtobufEndpointResolver),

                Register.EventInterceptors(new DefaultEventLoggingInterceptor(ctx.Resolve<ILogFactory>())),
                Register.CommandInterceptors(new DefaultCommandLoggingInterceptor(ctx.Resolve<ILogFactory>())),

                Register.BoundedContext(HistoryBoundedContext.Name)

                    .ListeningEvents(typeof(CashoutCompletedEvent), typeof(CashinCompletedEvent))
                    .From(BitcoinBoundedContext.Name)
                    .On(defaultRoute)
                    .WithEndpointResolver(sagasMessagePackEndpointResolver)
                    .WithProjection(typeof(TransactionHashProjection), BitcoinBoundedContext.Name)

                    .ListeningEvents(typeof(Lykke.Job.BlockchainCashinDetector.Contract.Events.CashinCompletedEvent))
                    .From(BlockchainCashinDetectorBoundedContext.Name)
                    .On(defaultRoute)
                    .WithEndpointResolver(sagasMessagePackEndpointResolver)
                    .WithProjection(typeof(TransactionHashProjection), BlockchainCashinDetectorBoundedContext.Name)

                    .ListeningEvents(typeof(Lykke.Job.SiriusDepositsDetector.Contract.Events.CashinCompletedEvent))
                    .From(SiriusDepositsDetectorBoundedContext.Name)
                    .On(defaultRoute)
                    .WithEndpointResolver(sagasMessagePackEndpointResolver)
                    .WithProjection(typeof(TransactionHashProjection), SiriusDepositsDetectorBoundedContext.Name)

                    .ListeningEvents(typeof(Lykke.Job.SiriusCashoutProcessor.Contract.Events.CashoutCompletedEvent))
                    .From(SiriusCashoutProcessorBoundedContext.Name)
                    .On(defaultRoute)
                    .WithEndpointResolver(sagasMessagePackEndpointResolver)
                    .WithProjection(typeof(TransactionHashProjection), SiriusCashoutProcessorBoundedContext.Name)

                    .ListeningEvents(
                        typeof(Lykke.Job.BlockchainCashoutProcessor.Contract.Events.CashoutCompletedEvent),
                        typeof(Lykke.Job.BlockchainCashoutProcessor.Contract.Events.CrossClientCashoutCompletedEvent),
                        typeof(Lykke.Job.BlockchainCashoutProcessor.Contract.Events.CashoutsBatchCompletedEvent))
                    .From(BlockchainCashoutProcessorBoundedContext.Name)
                    .On(defaultRoute)
                    .WithEndpointResolver(sagasMessagePackEndpointResolver)
                    .WithProjection(typeof(TransactionHashProjection), BlockchainCashoutProcessorBoundedContext.Name)

                    .ListeningCommands(typeof(CreateForwardCashinCommand), typeof(DeleteForwardCashinCommand))
                    .On("commands")
                    .WithCommandsHandler<ForwardWithdrawalCommandHandler>()
                    .PublishingEvents(typeof(ForwardCashinCreatedEvent), typeof(ForwardCashinDeletedEvent))
                    .With("events"),

            Register.BoundedContext("tx-handler.ethereum.commands")
                    .ListeningCommands(typeof(SaveEthInHistoryCommand), typeof(ProcessEthCoinEventCommand),
                        typeof(ProcessHotWalletErc20EventCommand))
                    .On("history")
                    .WithEndpointResolver(sagasMessagePackEndpointResolver)
                    .WithCommandsHandler<EthereumCommandHandler>());
        }
    }
}
