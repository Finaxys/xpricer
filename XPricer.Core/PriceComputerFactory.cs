using XPricer.Model;
using XPricer.Model.Product;

namespace XPricer.Core
{
    public class PriceComputerFactory : IPriceComputerFactory
    {
        public PriceComputer Create(PricingConfig config, Product product)
        {
            return new PriceComputer(config, product);
        }
    }
}