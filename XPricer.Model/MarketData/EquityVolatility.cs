using System;

namespace XPricer.Model.MarketData
{
    public class EquityVolatility : MarketData<StockKey>
    {
        public DateTime Date      { get; }
        public decimal Volatility { get; }

        public EquityVolatility(StockKey key, DateTime date, decimal volatility)
        {
            Type       = "Equity Volatility";
            Key        = key;
            Date       = date;
            Volatility = volatility;
        }

    }
}