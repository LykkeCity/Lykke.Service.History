using System;
using ProtoBuf;

namespace Antares.Service.History.Contracts.Cqrs.Commands
{
    [ProtoContract]
    public class DeleteForwardCashinCommand
    {
        [ProtoMember(1)]
        public Guid OperationId { get; set; }

        [ProtoMember(2)]
        public Guid WalletId { get; set; }
    }
}
