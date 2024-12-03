using System;
using ProtoBuf;

namespace Antares.Service.History.Contracts.Cqrs.Events
{
    [ProtoContract]
    public class ForwardCashinDeletedEvent
    {
        [ProtoMember(1)]
        public Guid OperationId { get; set; }

        [ProtoMember(2)]
        public Guid WalletId { get; set; }
    }
}
