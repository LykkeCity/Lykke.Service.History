using System;
using System.Collections.Generic;
using AutoMapper;
using Lykke.Service.History.Core.Domain.History;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;

namespace Lykke.Job.History.AutoMapper
{
    public class CashTransferConverter : ITypeConverter<CashTransferProcessedEvent, IEnumerable<BaseHistoryRecord>>
    {
        public IEnumerable<BaseHistoryRecord> Convert(CashTransferProcessedEvent source, IEnumerable<BaseHistoryRecord> destination,
            ResolutionContext context)
        {
            yield return new Cashout
            {
                Id = source.OperationId,
                WalletId = source.FromWalletId,
                Volume = -Math.Abs(source.Volume),
                Timestamp = source.Timestamp,
                AssetId = source.AssetId,
                FeeSize = source.FromWalletId == source.FeeSourceWalletId ? source.FeeSize : null,
                State = Service.History.Core.Domain.Enums.HistoryState.Finished
            };

            yield return new Cashin
            {
                Id = source.OperationId,
                WalletId = source.ToWalletId,
                Volume = Math.Abs(source.Volume),
                Timestamp = source.Timestamp,
                AssetId = source.AssetId,
                FeeSize = source.ToWalletId == source.FeeSourceWalletId ? source.FeeSize : null,
                State = Service.History.Core.Domain.Enums.HistoryState.Finished
            };
        }
    }
}
