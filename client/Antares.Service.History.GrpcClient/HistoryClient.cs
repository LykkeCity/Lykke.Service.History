using Antares.Service.History.GrpcClient.Common;
using Antares.Service.History.GrpcContract.Monitoring;
using Antares.Service.History.GrpcContract.Orders;
using Antares.Service.History.GrpcContract.Trades;

namespace Antares.Service.History.GrpcClient
{
    public class HistoryGrpcClient : BaseGrpcClient, IHistoryGrpcClient
    {
        public HistoryGrpcClient(string serverGrpcUrl) : base(serverGrpcUrl)
        {
            Monitoring = new MonitoringService.MonitoringServiceClient(_channel);
            Orders = new Orders.OrdersClient(_channel);
            Trades = new Trades.TradesClient(_channel);
            History = new GrpcContract.History.History.HistoryClient(_channel);
        }

        public MonitoringService.MonitoringServiceClient Monitoring { get; }
        public Orders.OrdersClient Orders { get; }
        public Trades.TradesClient Trades { get; }
        public GrpcContract.History.History.HistoryClient History { get; }
    }
}
