using SimpleTradingApp;
using SimpleTradingApp.Models;

var service = new TradeManagementService();
var trade = await service.CreateTrade(Currency.Eur, 9000.00m, Currency.Eth, 10, 3000.00m);
var result = await service.TryExecute(trade);

Console.WriteLine("Result is {0}", result);
