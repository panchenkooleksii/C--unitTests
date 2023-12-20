using RestEase;
using SimpleTradingApp.Models;

namespace SimpleTradingApp.Pricing;

public class PricingApiClient
{
    private readonly IPricingApi _pricingApi = RestClient.For<IPricingApi>(new Uri("https://api.kucoin.com/"));

    public async Task<decimal> GetPrice(Currency sourceCurrency, Currency destinationCurrency)
    {
        var invertResult = sourceCurrency == Currency.Eur;
        var response = invertResult
            ? await _pricingApi.Get($"{destinationCurrency}-{sourceCurrency}".ToUpper())
            : await _pricingApi.Get($"{sourceCurrency}-{destinationCurrency}".ToUpper());

        var result = invertResult
            ? response.Data.Sell
            : response.Data.Buy;

        if (!result.HasValue)
        {
            throw new ApplicationException($"Market data is not available for pair ({sourceCurrency}, {destinationCurrency})");
        }

        return result.Value;
    }
}