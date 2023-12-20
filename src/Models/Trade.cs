namespace SimpleTradingApp.Models;

public class Trade
{
    public Guid Id { get; set; }
    public TradeStatus Status { get; set; }
    public Currency SourceCurrency { get; set; }
    public decimal SourceCurrencyAmount { get; set; }
    public Currency DestinationCurrency { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public decimal? Price { get; set; }
    public decimal? ExecutedAmount { get; set; }
    public decimal? TargetPriceLimit { get; set; }
}