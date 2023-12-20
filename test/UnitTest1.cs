using SimpleTradingApp.Models;

namespace SimpleTradingApp.UnitTests;

public class TradeManagementServiceTests
{
    Trade result = new Trade
    {
        Id = Guid.NewGuid(),
        SourceCurrency = Currency.Btc,
        SourceCurrencyAmount = 30,
        DestinationCurrency = Currency.Eur,
        TargetPriceLimit = 200,
        Status = TradeStatus.Created,
        CreatedAt = DateTimeOffset.UtcNow,
        ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
    };
    TradeManagementService service;
    [OneTimeSetUp]
    public void Setup()
    {
        service = new TradeManagementService();
    }

    [Test]
    [TestCase(Currency.Btc,Currency.Eth)]
    [TestCase(Currency.Eth,Currency.Btc)]
    [TestCase(Currency.Btc,Currency.Btc)]
    [TestCase(Currency.Eth,Currency.Eth)]
    public void CreateTrade_InvalidCurrencyPair_ThrowsArgumentException(Currency source, Currency dest)
    {
        
        var ex = Assert.ThrowsAsync<ArgumentException>( () => service.CreateTrade(source, 9000.00m, dest, 10, 3000.00m));
        Assert.That(ex.Message, Is.EqualTo("At least one currency should be Fiat"));
    }
    [Test]
    [TestCase(Currency.Eur,Currency.Eur)]
    public void CreateTrade_InvalidCurrencyPair_ThrowsApplicationException(Currency source, Currency dest)
    {
        var ex = Assert.ThrowsAsync<ApplicationException>( () => service.CreateTrade(source, 9000.00m, dest, 10, 3000.00m));
        Assert.That(ex.Message, Is.EqualTo("Source and destination currency should be different"));
    }
    [Test]
    [TestCase(-1)]
    [TestCase(0)]
    public void CreateTrade_InvalidSourceAmount_ThrowsInvalidOperationException(decimal amount)
    {
        var ex = Assert.ThrowsAsync<InvalidOperationException>( () => service.CreateTrade(Currency.Btc, amount, Currency.Eur, 10, 3000.00m));
        Assert.That(ex.Message, Is.EqualTo("Amount for trade should be greater than 0"));
    }
    [Test]
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(31)]
    public void CreateTrade_InvalidExpiration_ThrowsApplicationException(int expirationInDays)
    {
        var ex = Assert.ThrowsAsync<ApplicationException>( () => service.CreateTrade(Currency.Btc, 3, Currency.Eur, expirationInDays, 3000.00m));
        Assert.That(ex.Message, Is.EqualTo("Expiration should be positive number of days that does not exceed 30"));
    }
}