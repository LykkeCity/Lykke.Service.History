using Antares.Service.History.GrpcContract.Monitoring;
using Antares.Service.History.GrpcContract.Orders;
using Antares.Service.History.GrpcContract.Trades;

namespace Antares.Service.History.GrpcClient
{
    public interface IHistoryGrpcClient
    {
        MonitoringService.MonitoringServiceClient Monitoring { get; }

        Orders.OrdersClient Orders { get; }

        Trades.TradesClient Trades { get; }

        GrpcContract.History.History.HistoryClient History { get; }
    }
}
