using System;

namespace XPricer.Model
{
    public class PricingConfig
    {
        public ulong    NumberOfPaths { get; }
        public DateTime PricingDate   { get; }

        public PricingConfig(ulong numberOfPaths, DateTime pricingDate)
        {
            NumberOfPaths = numberOfPaths;
            PricingDate = pricingDate;
        }
    }
}
