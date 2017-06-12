using XPricer.Model;
using XPricer.Model.Product;

namespace XPricer.Core
{
    public interface IPriceComputerFactory
    {
        IPriceComputer Create(PricingConfig config, Product product);
    }
}