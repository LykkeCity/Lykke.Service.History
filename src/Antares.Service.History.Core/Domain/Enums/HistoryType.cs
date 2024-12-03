namespace Antares.Service.History.Core.Domain.Enums
{
    public enum HistoryType
    {
        CashIn = 0,
        CashOut = 1,
        //Transfer = 2, reserved, but not used anymore
        Trade = 3,
        OrderEvent = 4
    }
}
