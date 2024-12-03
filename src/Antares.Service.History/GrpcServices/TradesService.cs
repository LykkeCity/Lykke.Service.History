using System;
using System.Linq;
using System.Threading.Tasks;
using Antares.Service.History.Contracts.Enums;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.Core.Domain.Orders;
using Antares.Service.History.GrpcContract.Common;
using Antares.Service.History.GrpcContract.Trades;
using Antares.Service.History.GrpcServices.Mappers;
using Grpc.Core;
using GetTradesResponse = Antares.Service.History.GrpcContract.Trades.GetTradesResponse;
using TradeType = Antares.Service.History.GrpcContract.Common.TradeType;

namespace Antares.Service.History.GrpcServices
{
    public class TradesService : Trades.TradesBase
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IHistoryRecordsRepository _historyRecordsRepository;

        public TradesService(
            IOrdersRepository ordersRepository,
            IHistoryRecordsRepository historyRecordsRepository)
        {
            _ordersRepository = ordersRepository;
            _historyRecordsRepository = historyRecordsRepository;
        }

        public override async Task<GetTradesResponse> GetTrades(GetTradesRequest request, ServerCallContext context)
        {
            var tradeType = request.TradeType;
            bool? onlyBuyTrades = null;
            if (tradeType == GrpcContract.Common.TradeType.Buy)
                onlyBuyTrades = true;
            if (tradeType == TradeType.Sell)
                onlyBuyTrades = false;

            var pagination = GrpcMapper.EnsurePagination(request.Pagination);

            var data = await _historyRecordsRepository
                .GetTradesByWalletAsync(
                    Guid.Parse(request.WalletId), 
                    pagination.Offset,
                    pagination.Limit,
                    request.AssetPairId,
                    request.AssetId,
                    request.From?.ToDateTime(),
                    request.To?.ToDateTime(), 
                    onlyBuyTrades);

            var items = data.Select(GrpcMapper.Map).ToArray();

            return new GetTradesResponse()
            {
                Pagination = new PaginatedInt32Response()
                {
                    Count = items.Length,
                    Offset = pagination.Offset + items.Length
                },
                Items = { items }
            };
        }

        public override async Task<GetTradesResponse> GetTradesByDates(GetTradesByDatesRequest request, ServerCallContext context)
        {
            var pagination = GrpcMapper.EnsurePagination(request.Pagination);
            var data = await _historyRecordsRepository.GetByDatesAsync(
                request.From.ToDateTime(),
                request.To.ToDateTime(),
                pagination.Offset,
                pagination.Limit);

            var items = data.Select(GrpcMapper.Map).ToArray();

            return new GetTradesResponse()
            {
                Pagination = new PaginatedInt32Response()
                {
                    Count = items.Length,
                    Offset = pagination.Offset + items.Length
                },
                Items = { items }
            };
        }

        public override async Task<GetTradesResponse> GetTradesByOrderId(GetTradesByOrderIdRequest request, ServerCallContext context)
        {
            var data = await _ordersRepository.GetTradesByOrderIdAsync(Guid.Parse(request.WalletId), Guid.Parse(request.Id));

            var items = data.Select(GrpcMapper.Map).ToArray();

            return new GetTradesResponse()
            {
                Pagination = new PaginatedInt32Response()
                {
                    Count = items.Length,
                    Offset = items.Length
                },
                Items = { items }
            };
        }
    }
}
