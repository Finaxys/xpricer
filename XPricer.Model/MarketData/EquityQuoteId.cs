namespace XPricer.Model.MarketData
{
    public class EquityQuoteId : MarketDataId
    {
        private const string EquityQuoteName = "Equity Quote";
        public EquityQuoteId(StockId stockId) : base(EquityQuoteName, new []{ stockId })
        {
        }
    }
}