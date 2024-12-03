using System;
using System.Linq;
using System.Threading.Tasks;
using Antares.Service.History.Core.Domain.Orders;
using Antares.Service.History.GrpcContract.Common;
using Antares.Service.History.GrpcContract.Orders;
using Antares.Service.History.GrpcServices.Mappers;
using Grpc.Core;

namespace Antares.Service.History.GrpcServices
{
    public class OrderService : Orders.OrdersBase
    {
        private readonly IOrdersRepository _ordersRepository;

        public OrderService(IOrdersRepository ordersRepository)
        {
            _ordersRepository = ordersRepository;
        }

        public override async Task<GetOrderListResponse> GetActiveOrders(GetActiveOrdersRequest request, ServerCallContext context)
        {
            var pagination = GrpcMapper.EnsurePagination(request.Pagination);
            var data = await _ordersRepository.GetOrdersAsync(
                Guid.Parse(request.WalletId),
                new[] { Core.Domain.Enums.OrderType.Limit,
                    Core.Domain.Enums.OrderType.StopLimit },
                new[] { Core.Domain.Enums.OrderStatus.Placed,
                    Core.Domain.Enums.OrderStatus.PartiallyMatched,
                    Core.Domain.Enums.OrderStatus.Pending },
                request.AssetPairId,
                pagination.Offset,
                pagination.Limit);

            var items = data.Select(x => GrpcMapper.MapOrder(x)).ToArray();

            return new GetOrderListResponse
            {
                Items = { items },
                Pagination = new PaginatedInt32Response
                {
                    Count = items.Length,
                    Offset = pagination.Offset + items.Length,
                }
            };
        }

        public override async Task<GetOrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
        {
            var order = await _ordersRepository.GetAsync(Guid.Parse(request.Id));

            return new GetOrderResponse
            {
                Item = order == null ? null : GrpcMapper.MapOrder(order)
            };
        }

        public override async Task<GetOrderListResponse> GetOrderList(GetOrderListRequest request, ServerCallContext context)
        {
            var status = request.Status.ToArray();

            if (status.Length == 0)
                status = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToArray();

            var type = request.Type.ToArray();

            if (type.Length == 0)
                type = Enum.GetValues(typeof(OrderType)).Cast<OrderType>().ToArray();

            var mappedType = type.Select(x => x switch
            {
                OrderType.UnknownType => Core.Domain.Enums.OrderType.Unknown,
                OrderType.Market => Core.Domain.Enums.OrderType.Market,
                OrderType.Limit => Core.Domain.Enums.OrderType.Limit,
                OrderType.StopLimit => Core.Domain.Enums.OrderType.StopLimit,
                _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
            }).ToArray();

            var mappedStatus = status.Select(x => x switch
            {
                OrderStatus.UnknownOrder => Core.Domain.Enums.OrderStatus.Unknown,
                OrderStatus.Placed => Core.Domain.Enums.OrderStatus.Placed,
                OrderStatus.PartiallyMatched => Core.Domain.Enums.OrderStatus.PartiallyMatched,
                OrderStatus.Matched => Core.Domain.Enums.OrderStatus.Matched,
                OrderStatus.Pending => Core.Domain.Enums.OrderStatus.Pending,
                OrderStatus.Cancelled => Core.Domain.Enums.OrderStatus.Cancelled,
                OrderStatus.Replaced => Core.Domain.Enums.OrderStatus.Replaced,
                OrderStatus.Rejected => Core.Domain.Enums.OrderStatus.Rejected,
                _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
            }).ToArray();

            var pagination = GrpcMapper.EnsurePagination(request.Pagination);

            var data = await _ordersRepository.GetOrdersAsync(
                Guid.Parse(request.WalletId),
                mappedType,
                mappedStatus,
                request.AssetPairId,
                pagination.Offset,
                pagination.Limit);

            var items = data.Select(GrpcMapper.MapOrder).ToArray();

            return new GetOrderListResponse
            {
                Items = { items },
                Pagination = new PaginatedInt32Response
                {
                    Count = items.Length,
                    Offset = pagination.Offset + items.Length
                }
            };
        }

        public override async Task<GetOrderListResponse> GetOrdersByDates(GetOrdersByDatesRequest request, ServerCallContext context)
        {
            var pagination = GrpcMapper.EnsurePagination(request.Pagination);
            var data = await _ordersRepository.GetOrdersByDatesAsync(
                request.From.ToDateTime(),
                request.To.ToDateTime(),
                pagination.Offset,
                pagination.Limit);

            var items = data.Select(GrpcMapper.MapOrder).ToArray();

            return new GetOrderListResponse
            {
                Items = { items },
                Pagination = new PaginatedInt32Response
                {
                    Count = items.Length,
                    Offset = pagination.Offset + items.Length
                }
            };
        }
    }
}
