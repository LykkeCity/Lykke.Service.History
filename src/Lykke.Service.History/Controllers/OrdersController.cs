using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.Log;
using Lykke.Service.History.Contracts.Orders;
using Lykke.Service.History.Core.Domain.Enums;
using Lykke.Service.History.Core.Domain.Orders;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Lykke.Service.History.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly ILogFactory _logFactory;

        public OrdersController(IOrdersRepository ordersRepository, ILogFactory logFactory)
        {
            _ordersRepository = ordersRepository;
            _logFactory = logFactory;
        }

        /// <summary>
        /// Get order by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [SwaggerOperation("GetOrder")]
        [ProducesResponseType(typeof(OrderModel), (int)HttpStatusCode.OK)]
        public async Task<OrderModel> GetOrder(Guid id)
        {
            return Mapper.Map<OrderModel>(await _ordersRepository.GetAsync(id));
        }

        /// <summary>
        /// Get order list by wallet id
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="status"></param>
        /// <param name="type"></param>
        /// <param name="assetPairId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [SwaggerOperation("GetOrderList")]
        [ProducesResponseType(typeof(IReadOnlyList<OrderModel>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<OrderModel>> GetOrders(
            [FromQuery] Guid walletId,
            [FromQuery(Name = "status")] OrderStatus[] status,
            [FromQuery(Name = "type")] OrderType[] type,
            string assetPairId = null,
            int offset = 0,
            int limit = 100)
        {
            if (status.Length == 0)
                status = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToArray();

            if (type.Length == 0)
                type = Enum.GetValues(typeof(OrderType)).Cast<OrderType>().ToArray();

            var data = await _ordersRepository.GetOrdersAsync(
                walletId,
                type,
                status,
                assetPairId,
                offset,
                limit);

            return Mapper.Map<IReadOnlyList<OrderModel>>(data);
        }

        /// <summary>
        /// Get order list by dates range.
        /// </summary>
        /// <param name="from">Inclusive range start</param>
        /// <param name="to">Exclusive range finish</param>
        /// <param name="offset">Number of skipped elements</param>
        /// <param name="limit">Max number of elements to be fetched</param>
        /// <returns>Orders from specified range.</returns>
        [HttpGet("bydates/{from}/{to}")]
        [SwaggerOperation("GetOrderList")]
        [ProducesResponseType(typeof(IReadOnlyList<OrderModel>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<OrderModel>> GetOrdersByDates(
            DateTime from,
            DateTime to,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 100)
        {
            if (from >= to)
                throw new ArgumentException($"Parameter '{nameof(from)}' must be earlier than parameter '{nameof(to)}'");

            var data = await _ordersRepository.GetOrdersByDatesAsync(
                from,
                to,
                offset,
                limit);

            return Mapper.Map<IReadOnlyList<OrderModel>>(data);
        }

        /// <summary>
        /// Get active orders by wallet id
        /// </summary>
        /// <param name="walletId"></param>
        /// <param name="assetPairId"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet("active")]
        [SwaggerOperation("GetActiveOrders")]
        [ProducesResponseType(typeof(IReadOnlyList<OrderModel>), (int)HttpStatusCode.OK)]
        public async Task<IReadOnlyList<OrderModel>> GetActiveOrders(
            [FromQuery] Guid walletId,
            string assetPairId = null,
            int offset = 0,
            int limit = 100)
        {
            var data = await _ordersRepository.GetOrdersAsync(
                walletId,
                new[] { OrderType.Limit, OrderType.StopLimit },
                new[] { OrderStatus.Placed, OrderStatus.PartiallyMatched, OrderStatus.Pending },
                assetPairId,
                offset,
                limit);

            return Mapper.Map<IReadOnlyList<OrderModel>>(data);
        }


        /// <summary>
        /// Start data fixing.
        /// </summary>
        /// <returns></returns>
        [HttpPost("replay")]
        [SwaggerOperation("Replay")]
        [ProducesResponseType(typeof(OrderModel), (int)HttpStatusCode.OK)]
        public async Task Replay(string meConnectionString)
        {
            var fixer = new Replay.OrdersStateFixer(_logFactory, meConnectionString, _ordersRepository);
            var task = fixer.FixClosedOrdersState();

            Task.Run(async () => await task);
        }
    }
}
