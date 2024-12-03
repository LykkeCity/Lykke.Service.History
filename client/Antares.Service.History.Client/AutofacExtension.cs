using System;
using System.Collections.Generic;
using Autofac;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Infrastructure;
using Newtonsoft.Json;

namespace Antares.Service.History.Client
{
    /// <summary>
    /// History service autofac registration extension
    /// </summary>
    [PublicAPI]
    public static class AutofacExtension
    {
        /// <summary>
        ///     Registers <see cref="IHistoryClient" /> in Autofac container using <see cref="HistoryServiceClientSettings" />.
        /// </summary>
        /// <param name="builder">Autofac container builder.</param>
        /// <param name="settings">History client settings.</param>
        /// <param name="builderConfigure">Optional <see cref="HttpClientGeneratorBuilder" /> configure handler.</param>
        public static void RegisterHistoryClient(
            [NotNull] this ContainerBuilder builder,
            [NotNull] HistoryServiceClientSettings settings,
            [CanBeNull] Func<HttpClientGeneratorBuilder, HttpClientGeneratorBuilder> builderConfigure = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.ServiceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.",
                    nameof(HistoryServiceClientSettings.ServiceUrl));

            var clientBuilder = Lykke.HttpClientGenerator.HttpClientGenerator.BuildForUrl(settings.ServiceUrl)
                .WithAdditionalCallsWrapper(new ExceptionHandlerCallsWrapper());

            clientBuilder = builderConfigure?.Invoke(clientBuilder) ?? clientBuilder.WithoutRetries();

            clientBuilder.WithJsonSerializerSettings(new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new HistoryJsonConverter() }
            });

            builder.RegisterInstance(new HistoryClient(clientBuilder.Create()))
                .As<IHistoryClient>()
                .SingleInstance();
        }
    }
}
