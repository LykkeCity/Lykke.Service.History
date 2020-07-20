using System.Collections.Generic;
using AutoMapper;
using Lykke.Service.History.Core.Domain.Orders;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;

namespace Lykke.Job.History.AutoMapper
{
    public class ExecutionConverter : ITypeConverter<ExecutionProcessedEvent, IEnumerable<Order>>
    {
        public IEnumerable<Order> Convert(ExecutionProcessedEvent source, IEnumerable<Order> destination,
            ResolutionContext context)
        {
            foreach (var item in source.Orders)
            {
                var order = Mapper.Map<Order>(item);

                order.SequenceNumber = source.SequenceNumber;

                foreach (var trade in order.Trades)
                    trade.OrderId = order.Id;

                yield return order;
            }
        }
    }
}
