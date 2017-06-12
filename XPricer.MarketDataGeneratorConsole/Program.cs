using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XPricer.MarketDataGenerator;

namespace XPricer.MarketDataGeneratorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Error.WriteLine("Starting data Generation...");
            FakeMarketData.CreateEquityQuotes();
            Console.Error.WriteLine("Data Generation Complete.");
        }
    }
}
