using System;
using ProtoBuf;

namespace Antares.Service.History.Contracts.Cqrs.Commands
{
    [ProtoContract]
    public class CreateForwardCashinCommand
    {
        [ProtoMember(1)]
        public Guid OperationId { get; set; }

        [ProtoMember(2)]
        public Guid WalletId { get; set; }

        [ProtoMember(3)]
        public decimal Volume { get; set; }

        [ProtoMember(4)]
        public string AssetId { get; set; }

        [ProtoMember(5)]
        public DateTime Timestamp { get; set; }
    }
}
