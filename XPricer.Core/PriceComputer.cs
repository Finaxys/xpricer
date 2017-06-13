﻿using System;
using System.Collections.Generic;
using NLog;
using QLNet;
using XPricer.Model;
using XPricer.Model.MarketData;
using XPricer.Model.Product;
using VanillaOption = XPricer.Model.Product.VanillaOption;
using System.Linq;

namespace XPricer.Core
{
    internal class PriceComputer : IPriceComputer
    {
        // Market Data
        public List<EquityQuote>      EquityQuotes       = new List<EquityQuote>();
        public List<EquityVolatility> EquityVolatilities = new List<EquityVolatility>();

        // ReSharper disable once InconsistentNaming
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(typeof(PriceComputer));

        public PricingConfig Config { get; }
        public Product Product { get; }

        public PriceComputer(PricingConfig config, Product product)
        {
            Config  = config;
            Product = product;
        }

        public PricingResult Compute()
        {
            var vanillaOption = Product as VanillaOption;
            if (vanillaOption == null)
            {
                throw new NotSupportedException($"The product {Product.GetType()} is not supported!");
            }

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
                    throw new ArgumentOutOfRangeException($"The vanilla option type {vanillaOption.OptionType} is not supported!");
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

            logger.Info("Option type = " + type);
            logger.Info("Maturity = " + maturity);
            logger.Info("Underlying price = " + underlying);
            logger.Info("Strike = " + strike);
            logger.Info("Risk-free interest rate = {0:0.000000%}", riskFreeRate);
            logger.Info("Dividend yield = {0:0.000000%}", dividendYield);
            logger.Info("Volatility = {0:0.000000%}", volatility);

            Exercise europeanExercise = new EuropeanExercise(maturity);

            var underlyingH = new Handle<Quote>(new SimpleQuote(Convert.ToDouble(underlying)));

            // bootstrap the yield/dividend/vol curves
            var flatTermStructure = new Handle<YieldTermStructure>(new FlatForward(Config.PricingDate, riskFreeRate, dayCounter));
            // ReSharper disable once InconsistentNaming
            var flatDividendTS = new Handle<YieldTermStructure>(new FlatForward(Config.PricingDate, dividendYield, dayCounter));
            // ReSharper disable once InconsistentNaming
            var flatVolTS = new Handle<BlackVolTermStructure>(new BlackConstantVol(Config.PricingDate, calendar, Convert.ToDouble(volatility), dayCounter));
            StrikedTypePayoff payoff = new PlainVanillaPayoff(type, strike);
            var bsmProcess = new BlackScholesMertonProcess(underlyingH, flatDividendTS, flatTermStructure, flatVolTS);

            // options
            var option = new QLNet.VanillaOption(payoff, europeanExercise);


            // Monte Carlo Method: MC (crude)
            const int timeSteps = 1;
            var mcSeed = Config.NumberOfPaths;
            var mcengine = new MakeMCEuropeanEngine<PseudoRandom>(bsmProcess)
               .withSteps(timeSteps)
               .withAbsoluteTolerance(0.02)
               .withSeed(mcSeed)
               .value();

            option.setPricingEngine(mcengine);

            var result = new PricingResult(option.NPV());

            logger.Info($"NPV: {result.Value}");

            return result;
        }

    }
}
