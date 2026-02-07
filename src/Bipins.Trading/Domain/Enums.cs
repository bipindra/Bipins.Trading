namespace Bipins.Trading.Domain;

public enum PositionSide
{
    Flat,
    Long,
    Short
}

public enum OrderSide
{
    Buy,
    Sell
}

public enum OrderType
{
    Market,
    Limit,
    Stop,
    StopLimit
}

public enum TimeInForce
{
    GTC,  // Good till cancelled
    IOC,  // Immediate or cancel
    FOK,  // Fill or kill
    Day
}

public enum SignalType
{
    Hold,
    EntryLong,
    ExitLong,
    EntryShort,
    ExitShort
}
