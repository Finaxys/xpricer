using System;

namespace XPricer.Model.MarketData
{
    public class EquityQuote : MarketData
    {
        public EquityQuoteId Id { get; }
        public DateTime Date { get; }
        public decimal Price { get; }

        public EquityQuote(EquityQuoteId id, DateTime date, decimal price)
        {
            Id = id;
            Date = date;
            Price = price;
        }

    }
}