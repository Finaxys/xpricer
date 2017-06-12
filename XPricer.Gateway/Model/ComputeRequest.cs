using System;
using Newtonsoft.Json;
using XPricer.Gateway.Controllers;
using XPricer.Model;
using XPricer.Model.Product;

namespace XPricer.Gateway.Model
{
    public class ComputeRequest
    {
        public PricingConfig Config { get; set; }
        public ProductContainer Product { get; set; }
    }

    public class ProductContainer
    {
        public VanillaOptionContainer Option { get; set; }
    }

    public class VanillaOptionContainer
    {
       public string Underlying { get; set; }
       public OptionType Type { get; set; }
       public double Strike { get; set; }
       public DateTime Maturity { get; set; }
        
    }

    public enum OptionType
    {
        Call,
        Put
    }

    public class PricingConfig
    {
        public ulong NumberOfPaths { get; set;  }
        public DateTime PricingDate { get; set; }
    }
}