using RestEase;

namespace SimpleTradingApp.Pricing;

[BasePath("/api/v1/market")]
public interface IPricingApi
{
    [Get("stats")]
    public Task<PriceResponse> Get([Query("symbol")] string symbol);
}