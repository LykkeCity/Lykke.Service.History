using System;
using System.Linq;
using System.Threading.Tasks;
using Antares.Service.History.GrpcClient;
using Antares.Service.History.GrpcContract.Common;
using Antares.Service.History.GrpcContract.History;
using Antares.Service.History.GrpcContract.Orders;
using Antares.Service.History.GrpcContract.Trades;

namespace ConsoleTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("PRESS ENTER TO START!");
            Console.ReadLine();

            var client = new HistoryGrpcClient("http://history.lykke-service.svc.cluster.local:5001");//"http://localhost:5001");

            var ordersByDates = await client.Orders.GetOrdersByDatesAsync(new GetOrdersByDatesRequest()
            {
                Pagination = new PaginationInt32()
                {
                    Limit = 100,
                    Offset = 0
                }
            });

            var walletId = ordersByDates.Items.First().WalletId;

            var orders = await client.Orders.GetOrderListAsync(new GetOrderListRequest()
            {
                WalletId = walletId
            });

            var order = await client.Orders.GetOrderAsync(new GetOrderRequest()
            {
                Id = orders.Items.First().Id
            });

            //var walletId = "608138d9-f3d7-4715-a2d1-9d8f3384d6dd";
            var history = await client.History.GetHistoryAsync(new HistoryGetHistoryRequest()
            {
                Pagination = new PaginationInt32()
                {
                    Limit = 100,
                    Offset = 0
                },
                AssetPairId = "EURCHF",
                WalletId = walletId
            });

            var hist1 = history.Items.First();
            var historyItem = await client.History.GetHistoryItemAsync(new GetHistoryItemRequest() { WalletId = hist1.WalletId, Id = hist1.Id });

            var activeOrders = await client.Orders.GetActiveOrdersAsync(new GetActiveOrdersRequest() {WalletId = walletId});

            var trades = await client.Trades.GetTradesAsync(new GetTradesRequest() {WalletId = walletId});

            var trades1 = await client.Trades.GetTradesByOrderIdAsync(new GetTradesByOrderIdRequest()
            {
                WalletId = walletId,
                Id = order.Item.Id
            });
        }
    }
}
