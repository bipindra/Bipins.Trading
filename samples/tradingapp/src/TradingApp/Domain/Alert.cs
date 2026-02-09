using Bipins.Trading.Domain;

namespace TradingApp.Domain;

public class Alert
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public AlertType AlertType { get; set; }
    public string? Payload { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? TriggeredAt { get; set; }
    
    // New fields for enhanced alert configuration
    public decimal? Threshold { get; set; } // For RSI or price thresholds
    public ComparisonType? ComparisonType { get; set; } // CrossesOver, CrossesBelow, Above, Below
    public string? Timeframe { get; set; } // 1Min, 3Min, 5Min, 15Min, 1Hour, 1Day, etc.
    
    // Order execution configuration
    public bool EnableAutoExecute { get; set; } // Whether to automatically execute order on trigger
    public decimal? OrderQuantity { get; set; } // Quantity for the order (default: 1)
    public OrderType? OrderType { get; set; } // Order type (default: Market)
    public OrderSide? OrderSideOverride { get; set; } // Override default side (null = use alert type default)
    public decimal? OrderLimitPrice { get; set; } // Limit price for limit orders
    public decimal? OrderStopPrice { get; set; } // Stop price for stop orders
    public TimeInForce? OrderTimeInForce { get; set; } // Time in force (default: Day)
}

public enum ComparisonType
{
    Above = 0,        // Value is above threshold
    Below = 1,        // Value is below threshold
    CrossesOver = 2,  // Value crosses over threshold (was below, now above)
    CrossesBelow = 3  // Value crosses below threshold (was above, now below)
}
