using System;

namespace XPricer.Model.MarketData
{
    public class EquityQuote : MarketData<StockKey>
    {
        public DateTime Date  { get; }
        public decimal  Price { get; }

        public EquityQuote(StockKey key, DateTime date, decimal price)
        {
            Type  = "Equity Quote";
            Key   = key;
            Date  = date;
            Price = price;
        }

    }
}