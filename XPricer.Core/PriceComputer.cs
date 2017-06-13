using NLog;
using QLNet;
using System;
using System.Collections.Generic;
using System.Linq;
using XPricer.Model;
using XPricer.Model.MarketData;
using XPricer.Model.Product;
using VanillaOption = XPricer.Model.Product.VanillaOption;

namespace XPricer.Core
{
    internal class PriceComputer : IPriceComputer
    {
        // Config
        public PricingConfig Config  { get; }
        public Product       Product { get; }

         // Market Data
        public List<EquityQuote>      EquityQuotes       = new List<EquityQuote>();
        public List<EquityVolatility> EquityVolatilities = new List<EquityVolatility>();

        private static readonly Logger _log = LogManager.GetCurrentClassLogger(typeof(PriceComputer));

        // .ctor
        public PriceComputer(PricingConfig config, Product product)
        {
            Config  = config;
            Product = product;
        }

        // compute the product
        public PricingResult Compute()
        {
            var vanillaOption = Product as VanillaOption;
            if (vanillaOption == null)
                throw new NotSupportedException($"The product {Product.GetType()} is not supported!");

            var calendar = new TARGET();
            Settings.setEvaluationDate(Config.PricingDate);
            
            Option.Type type;
            switch (vanillaOption.OptionType)
            {
                case OptionType.Put:
                    type = Option.Type.Put;
                    break;
                case OptionType.Call:
                    type = Option.Type.Call;
                    break;
                default:
                    throw new ArgumentException($"The vanilla option type {vanillaOption.OptionType} is not supported!");
            }

            var underlying = EquityQuotes.First(q => q.Key.Id == vanillaOption.Underlying
                                                  && q.Date.Year == Config.PricingDate.Year
                                                  && q.Date.Month == Config.PricingDate.Month
                                                  && q.Date.Day == Config.PricingDate.Day)
                                         .Price;
            var strike = vanillaOption.Strike;
            var dividendYield = 0.00;
            var riskFreeRate = 0.06;
            var volatility = EquityVolatilities.First(v => v.Key.Id == vanillaOption.Underlying
                                                        && v.Date.Year == Config.PricingDate.Year
                                                        && v.Date.Month == Config.PricingDate.Month
                                                        && v.Date.Day == Config.PricingDate.Day)
                                               .Volatility;
            Date maturity = vanillaOption.Maturity;
            DayCounter dayCounter = new Actual365Fixed();

            _log.Info("Option type = " + type);
            _log.Info("Maturity = " + maturity);
            _log.Info("Underlying price = " + underlying);
            _log.Info("Strike = " + strike);
            _log.Info("Risk-free interest rate = {0:0.000000%}", riskFreeRate);
            _log.Info("Dividend yield = {0:0.000000%}", dividendYield);
            _log.Info("Volatility = {0:0.000000%}", volatility);

            Exercise europeanExercise = new EuropeanExercise(maturity);

            var underlyingH = new Handle<Quote>(new SimpleQuote(Convert.ToDouble(underlying)));

            // bootstrap the yield/dividend/vol curves
            var flatTermStructure    = new Handle<YieldTermStructure>(new FlatForward(Config.PricingDate, riskFreeRate, dayCounter));
            var flatDividendTs       = new Handle<YieldTermStructure>(new FlatForward(Config.PricingDate, dividendYield, dayCounter));
            var flatVolTs            = new Handle<BlackVolTermStructure>(new BlackConstantVol(Config.PricingDate, calendar, Convert.ToDouble(volatility), dayCounter));
            StrikedTypePayoff payoff = new PlainVanillaPayoff(type, strike);
            var bsmProcess           = new BlackScholesMertonProcess(underlyingH, flatDividendTs, flatTermStructure, flatVolTs);

            // options
            var option = new QLNet.VanillaOption(payoff, europeanExercise);


            // Monte Carlo Method: MC (crude)
            const int timeSteps = 1;
            var mcSeed = Config.NumberOfPaths;
            var mcengine = new MakeMCEuropeanEngine<PseudoRandom>(bsmProcess).withSteps(timeSteps)
                                                                             .withAbsoluteTolerance(0.02)
                                                                             .withSeed(mcSeed)
                                                                             .value();
            option.setPricingEngine(mcengine);

            var result = new PricingResult(option.NPV());

            _log.Info($"NPV: {result.Value}");

            return result;
        }
    }
}
