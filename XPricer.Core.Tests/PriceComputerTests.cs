using System;
using NUnit.Framework;
using XPricer.Framework;
using XPricer.Model;
using XPricer.Model.Product;

namespace XPricer.Core.Tests
{
    [TestFixture]
    public class PriceComputerTests
    {
        [SetUp]
        public void Setup()
        {
            var loggerConfigurator = new LoggerConfigurator();
            loggerConfigurator.Configure();
        }

        [Test]
        public void ShouldPriceVanillaOption()
        {
            var option = new VanillaOption("MSFT", OptionType.Call, 73, DateTime.Now.AddMonths(6));
            var config = new PricingConfig(10000, DateTime.Now);
            var priceComputerFactory = new PriceComputerFactory();
            var priceComputer = priceComputerFactory.Create(config, option);
            var result = priceComputer.Compute();
            Assert.IsNotNull(result);
        }
    
    }
}
