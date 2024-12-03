using System;
using System.Linq;
using System.Threading.Tasks;
using Antares.Service.History.Core.Domain.History;
using Antares.Service.History.GrpcContract.Common;
using Antares.Service.History.GrpcContract.History;
using Antares.Service.History.GrpcServices.Mappers;
using Grpc.Core;
using GetHistoryItemResponse = Antares.Service.History.GrpcContract.History.GetHistoryItemResponse;

namespace Antares.Service.History.GrpcServices
{
    public class HistoryService : GrpcContract.History.History.HistoryBase
    {
        private readonly IHistoryRecordsRepository _historyRecordsRepository;

        public HistoryService(IHistoryRecordsRepository historyRecordsRepository)
        {
            _historyRecordsRepository = historyRecordsRepository;
        }

        public override async Task<HistoryGetHistoryResponse> GetHistory(HistoryGetHistoryRequest request, ServerCallContext context)
        {
            var type = request.Type.ToArray();

            if (type.Length == 0)
                type = Enum.GetValues(typeof(GrpcContract.Common.HistoryType)).Cast<GrpcContract.Common.HistoryType>().ToArray();

            var mappedType = type.Select(x => x switch
            {
                GrpcContract.Common.HistoryType.CashIn => Antares.Service.History.Core.Domain.Enums.HistoryType.CashIn,
                GrpcContract.Common.HistoryType.CashOut => Antares.Service.History.Core.Domain.Enums.HistoryType.CashOut,
                GrpcContract.Common.HistoryType.Trade => Antares.Service.History.Core.Domain.Enums.HistoryType.Trade,
                GrpcContract.Common.HistoryType.OrderEvent => Antares.Service.History.Core.Domain.Enums.HistoryType.OrderEvent,

                _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
            }).ToArray();


            var pagination = GrpcMapper.EnsurePagination(request.Pagination);

            var data = await _historyRecordsRepository.GetByWalletAsync(
                Guid.Parse(request.WalletId),
                mappedType,
                pagination.Offset,
                pagination.Limit,
                request.AssetPairId,
                request.AssetId,
                request.From?.ToDateTime(),
                request.To?.ToDateTime());

            var mappedItems = GrpcMapper.Map(data);
            return new HistoryGetHistoryResponse()
            {
                Items = { mappedItems },
                Pagination = new PaginatedInt32Response()
                {
                    Count = mappedItems.Count,
                    Offset = pagination.Offset + mappedItems.Count,
                }
            };
        }

        public override async Task<GetHistoryItemResponse> GetHistoryItem(GetHistoryItemRequest request, ServerCallContext context)
        {
            var item = await _historyRecordsRepository.GetAsync(Guid.Parse( request.Id), Guid.Parse(request.WalletId));

            return new GetHistoryItemResponse()
            {
                Item = item != null ? GrpcMapper.Map(item): null
            };
        }
    }
}
