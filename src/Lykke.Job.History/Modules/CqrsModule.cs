using System;
using System.Collections.Generic;
using Autofac;
using Lykke.Bitcoin.Contracts;
using Lykke.Bitcoin.Contracts.Events;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Cqrs.Middleware.Logging;
using Lykke.Job.BlockchainCashinDetector.Contract;
using Lykke.Job.BlockchainCashoutProcessor.Contract;
using Lykke.Job.History.Workflow.ExecutionProcessing;
using Lykke.Job.History.Workflow.Handlers;
using Lykke.Job.History.Workflow.Projections;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Sdk;
using Lykke.Service.History.Contracts.Cqrs;
using Lykke.Service.History.Contracts.Cqrs.Commands;
using Lykke.Service.History.Contracts.Cqrs.Events;
using Lykke.Service.History.Core.Settings;
using Lykke.Service.PostProcessing.Contracts.Cqrs;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using Lykke.SettingsReader;
using RabbitMQ.Client;

namespace Lykke.Job.History.Modules
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
            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>()
                .SingleInstance();

            var rabbitMqSettings = new ConnectionFactory
            {
                Uri = new Uri(_settings.Cqrs.RabbitConnString)
            };
            var rabbitMqEndpoint = rabbitMqSettings.Endpoint.ToString();

            builder.RegisterType<StartupManager>().As<IStartupManager>();

            builder.RegisterType<ExecutionQueueReader>()
                .WithParameter(TypedParameter.From(_settings.Cqrs.RabbitConnString))
                .WithParameter(new NamedParameter("prefetchCount", _settings.RabbitPrefetchCount))
                .WithParameter(new NamedParameter("batchCount", _settings.PostgresOrdersBatchSize))
                .SingleInstance();

            builder.RegisterType<OrderEventQueueReader>()
                .WithParameter(TypedParameter.From(_settings.Cqrs.RabbitConnString))
                .WithParameter(new NamedParameter("prefetchCount", _settings.RabbitPrefetchCount))
                .WithParameter(new NamedParameter("batchCount", _settings.PostgresOrdersBatchSize))
                .SingleInstance();

            builder.RegisterType<CashInProjection>();
            builder.RegisterType<CashOutProjection>();
            builder.RegisterType<CashTransferProjection>();
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
                    cqrsEngine.StartPublishers();
                    return cqrsEngine;
                })
                .As<ICqrsEngine>()
                .AutoActivate()
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

                Register.BoundedContext(HistoryBoundedContext.Name)
                    .ListeningEvents(typeof(CashInProcessedEvent))
                    .From(PostProcessingBoundedContext.Name)
                    .On(defaultRoute)
                    .WithProjection(typeof(CashInProjection), PostProcessingBoundedContext.Name)

                    .ListeningEvents(typeof(CashOutProcessedEvent))
                    .From(PostProcessingBoundedContext.Name)
                    .On(defaultRoute)
                    .WithProjection(typeof(CashOutProjection), PostProcessingBoundedContext.Name)

                    .ListeningEvents(typeof(CashTransferProcessedEvent))
                    .From(PostProcessingBoundedContext.Name)
                    .On(defaultRoute)
                    .WithProjection(typeof(CashTransferProjection), PostProcessingBoundedContext.Name)

                    .ListeningEvents(typeof(CashoutCompletedEvent), typeof(CashinCompletedEvent))
                    .From(BitcoinBoundedContext.Name)
                    .On(defaultRoute)
                    .WithEndpointResolver(sagasMessagePackEndpointResolver)
                    .WithProjection(typeof(TransactionHashProjection), BitcoinBoundedContext.Name)

                    .ListeningEvents(typeof(Job.BlockchainCashinDetector.Contract.Events.CashinCompletedEvent))
                    .From(BlockchainCashinDetectorBoundedContext.Name)
                    .On(defaultRoute)
                    .WithEndpointResolver(sagasMessagePackEndpointResolver)
                    .WithProjection(typeof(TransactionHashProjection), BlockchainCashinDetectorBoundedContext.Name)

                    .ListeningEvents(
                        typeof(Job.BlockchainCashoutProcessor.Contract.Events.CashoutCompletedEvent),
                        typeof(Job.BlockchainCashoutProcessor.Contract.Events.CrossClientCashoutCompletedEvent),
                        typeof(Job.BlockchainCashoutProcessor.Contract.Events.CashoutsBatchCompletedEvent))
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
