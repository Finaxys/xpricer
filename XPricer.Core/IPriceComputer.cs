using System.Collections.Generic;
using XPricer.Model;
using XPricer.Model.MarketData;

namespace XPricer.Core
{
    public interface IPriceComputer
    {
        PricingResult Compute();

   

    }
}