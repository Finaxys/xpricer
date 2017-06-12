namespace XPricer.Model
{
    public class ComputeRequest
    {
        public Product.Product Product { get;  }
        public PricingConfig Config { get;  }

        public ComputeRequest(PricingConfig config, Product.Product product)
        {
            Config = config;
            Product = product;
        }
    }
}