using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.History.Replay
{
    public class MatchingEngineEvent : TableEntity
    {
        public string Message { get; set; }
        public string MessageId { get; set; }
        public DateTime MessageTimestamp { get; set; }
        public string RequestId { get; set; }
        public long SequenceNumber { get; set; }
    }
}
