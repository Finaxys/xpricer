using System.Collections.Generic;

namespace XPricer.Model.MarketData
{
    public abstract class MarketDataId
    {
        public string Type { get; }
        public IEnumerable<Key> Keys { get; }

        public MarketDataId(string type, IEnumerable<Key> keys)
        {
            this.Type = type;
            this.Keys = keys;
        }
        
    }
}