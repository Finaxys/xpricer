namespace XPricer.Model.MarketData
{
    public class StockId : Key
    {
        public string Name { get; }

        public StockId(string name)
        {
            this.Name = name;
        }
        
    }
}