namespace XPricer.Model.MarketData
{
    public abstract class MarketData<TKey>
    {
        public TKey   Key  { get; set; }
        public string Type { get; set; }
    }
}