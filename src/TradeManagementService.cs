using Microsoft.EntityFrameworkCore;
using SimpleTradingApp.Models;
using SimpleTradingApp.Pricing;

namespace SimpleTradingApp;

public class TradeManagementService
{
    private readonly TradingAppDbContext _db = TradingAppDbContext.Create();
    private readonly PricingApiClient _pricingApiClient = new PricingApiClient();
    
    public async Task<Trade> CreateTrade(
        Currency sourceCurrency,
        decimal sourceCurrencyAmount,
        Currency destinationCurrency,
        int expirationInDays,
        decimal? targetPriceLimit = null)
    {
        if (sourceCurrency != Currency.Eur && destinationCurrency != Currency.Eur)
        {
            throw new ArgumentException("At least one currency should be Fiat");
        }

        if (sourceCurrency == destinationCurrency)
        {
            throw new ApplicationException("Source and destination currency should be different");
        }

        if (sourceCurrencyAmount <= 0)
        {
            throw new InvalidOperationException("Amount for trade should be greater than 0");
        }

        if (expirationInDays is <= 0 or > 30)
        {
            throw new ApplicationException("Expiration should be positive number of days that does not exceed 30");
        }

        var result = new Trade
        {
            Id = Guid.NewGuid(),
            SourceCurrency = sourceCurrency,
            SourceCurrencyAmount = sourceCurrencyAmount,
            DestinationCurrency = destinationCurrency,
            TargetPriceLimit = targetPriceLimit,
            Status = TradeStatus.Created,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(expirationInDays),
        };

        _db.Trades.Add(result);
        await _db.SaveChangesAsync();

        return result;
    }

    public async Task CancelTrade(Guid tradeId)
    {
        var trade = await _db.Trades.FirstOrDefaultAsync(x => x.Id == tradeId);
        if (trade is null)
        {
            throw new ApplicationException($"Trade with tradeId={tradeId} not found");
        }

        if (trade.Status != TradeStatus.Canceled)
        {
            throw new ApplicationException($"Trade in status {trade.Status} can't be cancelled");
        }

        trade.Status = TradeStatus.Canceled;

        await _db.SaveChangesAsync();
    }

    public async Task SetExpiredTradesStatus()
    {
        var utcNow = DateTimeOffset.UtcNow;
        var trades = await _db.Trades
            .Where(x => x.Status == TradeStatus.Created && x.ExpiresAt < utcNow)
            .ToListAsync();

        trades.ForEach(x => x.Status = TradeStatus.Expired);
        
        await _db.SaveChangesAsync();
    }

    public async Task<bool> TryExecute(Trade trade)
    {
        if (trade is null)
        {
            throw new ArgumentNullException(nameof(trade));
        }

        if (trade.Status != TradeStatus.Created)
        {
            throw new ApplicationException($"Trade in status={trade.Status} is not eligible for execution");
        }

        var price = await _pricingApiClient.GetPrice(trade.SourceCurrency, trade.DestinationCurrency);

        if (trade.TargetPriceLimit is null ||
            (trade.SourceCurrency == Currency.Eur && price < trade.TargetPriceLimit) ||
            (trade.SourceCurrency != Currency.Eur && price > trade.TargetPriceLimit))
        {
            ExecuteInternal(trade, price);
            return true;
        }

        return false;
    }

    private void ExecuteInternal(Trade trade, decimal price)
    {
        trade.Price = price;
        trade.Status = TradeStatus.Executed;
        if (trade.SourceCurrency == Currency.Eur)
        {
            trade.ExecutedAmount = trade.SourceCurrencyAmount / price;
        }
        else
        {
            trade.ExecutedAmount = trade.SourceCurrencyAmount * price;
        }
    }
}