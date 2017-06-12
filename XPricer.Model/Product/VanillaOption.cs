using System;

namespace XPricer.Model.Product
{
    public class VanillaOption : Product
    {
       public string Underlying { get; }
       public OptionType OptionType { get; }
       public double Strike { get; }
       public DateTime Maturity { get; }

       public VanillaOption(
          string underlying,
          OptionType optionType,
          double strike,
          DateTime maturity
          )
       {
          this.Underlying = underlying;
          this.OptionType = optionType;
          this.Strike = strike;
          this.Maturity = maturity;
       }
    }
}
