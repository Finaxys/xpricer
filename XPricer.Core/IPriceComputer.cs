using XPricer.Model;

namespace XPricer.Core
{
    public interface IPriceComputer
    {
        PricingResult Compute();

    }
}