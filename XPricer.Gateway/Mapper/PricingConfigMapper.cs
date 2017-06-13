using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XPricer.Gateway.Model;

namespace XPricer.Gateway.Mapper
{
    public static class ComputeRequestMapper
    {
        public static XPricer.Model.ComputeRequest ToInternal(ComputeRequest request)
        {
            return new XPricer.Model.ComputeRequest(
                PricingConfigMapper.ToInternal(request.Config),
                ProductMapper.ToInternal(request.Product));
        }
    }

    public static class RequestIdMapper
    {
        public static RequestId ToExternal(XPricer.Model.RequestId request)
        {
            return new RequestId(request.Id);
        }
    }

    public static class PricingConfigMapper
    {
        public static XPricer.Model.PricingConfig ToInternal(PricingConfig config)
        {
            return new XPricer.Model.PricingConfig(config.NumberOfPaths, config.PricingDate);
        }
    }

    public static class ProductMapper
    {
        public static XPricer.Model.Product.Product ToInternal(ProductContainer container)
        {
            return new XPricer.Model.Product.VanillaOption(
                underlying: container.Option.Underlying,
                optionType: OptionTypeMapper.ToInternal(container.Option.Type),
                strike: container.Option.Strike,
                maturity: container.Option.Maturity
            );
        }
    }

    public static class OptionTypeMapper
    {
        public static XPricer.Model.Product.OptionType ToInternal(OptionType type)
        {
            switch (type)
            {
                    
                case OptionType.Call:
                    return XPricer.Model.Product.OptionType.Call;
                case OptionType.Put:
                    return XPricer.Model.Product.OptionType.Put;
                default:
                    throw new ArgumentOutOfRangeException($"Unable to map type {type}");
            }
        }
    }
}
