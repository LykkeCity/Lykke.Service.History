using Lykke.HttpClientGenerator;

namespace Antares.Service.History.Client
{
    /// <inheritdoc />
    /// <summary>
    ///     History API aggregating interface.
    /// </summary>
    public class HistoryClient : IHistoryClient
    {
        /// <summary>C-tor</summary>
        public HistoryClient(IHttpClientGenerator httpClientGenerator)
        {
            HistoryApi = httpClientGenerator.Generate<IHistoryApi>();
            OrdersApi = httpClientGenerator.Generate<IOrdersApi>();
            TradesApi = httpClientGenerator.Generate<ITradesApi>();
        }
        // Note: Add similar Api properties for each new service controller

        /// <inheritdoc />
        public IHistoryApi HistoryApi { get; }

        /// <inheritdoc />
        public IOrdersApi OrdersApi { get; }
        
        /// <inheritdoc />
        public ITradesApi TradesApi { get; }
    }
}
