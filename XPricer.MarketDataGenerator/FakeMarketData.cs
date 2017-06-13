using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XPricer.Model.MarketData;

namespace XPricer.MarketDataGenerator
{
    public class FakeMarketData
    {
        // public

        public static void CreateEquityMarketData()
        {
            var eq = GenerateEquityQuotes("MSF", new DateTime(2016, 06, 13), DateTime.Now, 44, 65, 44, 0.12M); // Microsoft
            File.WriteAllText("MSF-QUOTES.csv", Serialize(eq));
            eq = GetEquityQuotes("MSF");
            var ev = GenerateEquityVolatilities(eq, 0.12M);
            File.WriteAllText("MSF-VOLATILITIES.csv", Serialize(ev));
            ev = GetEquityVolatilities("MSF");

            eq = GenerateEquityQuotes("FP", new DateTime(2016, 06, 13), DateTime.Now, 38, 48, 40, 0.1M); // Total
            File.WriteAllText("FP-QUOTES.csv", Serialize(eq));
            ev = GenerateEquityVolatilities(eq, 0.1M);
            File.WriteAllText("FP-VOLATILITIES.csv", Serialize(ev));

            eq = GenerateEquityQuotes("AMZ", new DateTime(2016, 06, 13), DateTime.Now, 636, 900, 636, 0.05M); // Amazon
            File.WriteAllText("AMZ-QUOTES.csv", Serialize(eq));
            ev = GenerateEquityVolatilities(eq, 0.05M);
            File.WriteAllText("AMZ-VOLATILITIES.csv", Serialize(ev));
        }

        public static List<EquityQuote> GetEquityQuotes(string equityId)
        {
            if (String.IsNullOrWhiteSpace(equityId))
                throw new ArgumentException("Must not be null or whitespaces", "equityQuoteId");
            equityId = equityId.ToUpperInvariant();
            if (equityId != "MSF" && equityId != "FP" && equityId != "AMZ")
                throw new ArgumentException("Must be MSF, FP or AMZ", "equityQuoteId");
            return DeserializeQuotes(File.ReadAllText(equityId + "-QUOTES.csv"));
        }

        public static List<EquityVolatility> GetEquityVolatilities(string equityId)
        {
            if (String.IsNullOrWhiteSpace(equityId))
                throw new ArgumentException("Must not be null or whitespaces", "equityId");
            equityId = equityId.ToUpperInvariant();
            if (equityId != "MSF" && equityId != "FP" && equityId != "AMZ")
                throw new ArgumentException("Must be MSF, FP or AMZ", "equityId");
            return DeserializeVolatilities(File.ReadAllText(equityId + "-VOLATILITIES.csv"));
        }

        // private

        private static List<EquityQuote> GenerateEquityQuotes(string equityQuoteId, DateTime start, DateTime end, decimal priceMin, decimal priceMax, decimal priceStart, decimal maxDailyVolatility)
        {
            var r = new Random();
            var key = new StockKey { Id = equityQuoteId };
            var quotes = new List<EquityQuote>();
            var d = start;
            var p = priceStart;
            quotes.Add(new EquityQuote(key, start, priceStart));
            d = d.AddDays(1);
            while (d <= end)
            {
                var v = Convert.ToDecimal(r.NextDouble()) * maxDailyVolatility;
                if (r.Next(-1, 1) < 0)
                    v = -v;

                var np = p * (1 - v);
                if (np < priceMin || np > priceMax)
                    np = p * (1 + v);

                p = np;
                quotes.Add(new EquityQuote(key, d, p));

                d = d.AddDays(1);
            }
            return quotes;
        }

        private static List<EquityVolatility> GenerateEquityVolatilities(List<EquityQuote> equityQuotes, decimal maxDailyVolatility)
        {
            var volatilities = new List<EquityVolatility>();
            var pp = equityQuotes[0].Price * (1 - maxDailyVolatility / 2);
            foreach (var eq in equityQuotes)
            {
                volatilities.Add(new EquityVolatility(eq.Key, eq.Date, Math.Abs(1 - (eq.Price / pp))));
                pp = eq.Price;
            }
            return volatilities;
        }

        private static string Serialize(List<EquityQuote> equityQuotes)
        {
            var sb = new StringBuilder();
            foreach (var eq in equityQuotes)
                sb.AppendLine(eq.Key.Id + ";" + eq.Date + ";" + eq.Price);
            return sb.ToString();
        }

        private static string Serialize(List<EquityVolatility> equityVolatilities)
        {
            var sb = new StringBuilder();
            foreach (var ev in equityVolatilities)
                sb.AppendLine(ev.Key.Id + ";" + ev.Date + ";" + ev.Volatility);
            return sb.ToString();
        }

        private static List<EquityQuote> DeserializeQuotes(string text)
        {
            StockKey key = null;
            var equityQuotes = new List<EquityQuote>();
            using (StringReader sr = new StringReader(text))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var item = line.Split(new[] { ';' });
                    if (key == null)
                        key = new StockKey { Id = item[0] };
                    equityQuotes.Add(new EquityQuote(key, DateTime.Parse(item[1]), Decimal.Parse(item[2])));
                }
            }
            return equityQuotes;
        }

        private static List<EquityVolatility> DeserializeVolatilities(string text)
        {
            StockKey key = null;
            var equityVolatilities = new List<EquityVolatility>();
            using (StringReader sr = new StringReader(text))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var item = line.Split(new[] { ';' });
                    if (key == null)
                        key = new StockKey { Id = item[0] };
                    equityVolatilities.Add(new EquityVolatility(key, DateTime.Parse(item[1]), Decimal.Parse(item[2])));
                }
            }
            return equityVolatilities;
        }
    }
}