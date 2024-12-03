namespace Antares.Job.History.RabbitSubscribers.Models.Enums
{
    public enum OrderStatus
    {
        Unknown,
        Placed,
        PartiallyMatched,
        Matched,
        Pending,
        Cancelled,
        Replaced,
        Rejected
    }
}
