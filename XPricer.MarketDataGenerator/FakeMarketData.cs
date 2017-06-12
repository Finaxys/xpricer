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

        public static void CreateEquityQuotes()
        {
            var eq = GenerateEquityQuotes("MSF", new DateTime(2016, 06, 13), DateTime.Now, 44, 65, 44, 0.12M); // Microsoft
            File.WriteAllText("MSF.csv", Serialize(eq));
            eq = GetEquityQuotes("MSF");

            eq = GenerateEquityQuotes("FP", new DateTime(2016, 06, 13), DateTime.Now, 38, 48, 40, 0.1M); // Total
            File.WriteAllText("FP.csv", Serialize(eq));

            eq = GenerateEquityQuotes("AMZ", new DateTime(2016, 06, 13), DateTime.Now, 636, 900, 636, 0.05M); // Amazon
            File.WriteAllText("AMZ.csv", Serialize(eq));
        }

        public static List<EquityQuote> GetEquityQuotes(string equityQuoteId)
        {
            if (String.IsNullOrWhiteSpace(equityQuoteId))
                throw new ArgumentException("Must not be null or whitespaces", "equityQuoteId");
            equityQuoteId = equityQuoteId.ToUpperInvariant();
            if (equityQuoteId != "MSF" && equityQuoteId != "FP" && equityQuoteId != "AMZ")
                throw new ArgumentException("Must be MSF, FP or AMZ", "equityQuoteId");
            return Deserialize(File.ReadAllText(equityQuoteId + ".csv"));
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

        private static string Serialize(List<EquityQuote> equityQuotes)
        {
            var sb = new StringBuilder();
            foreach (var eq in equityQuotes)
                sb.AppendLine(eq.Key.Id + ";" + eq.Date + ";" + eq.Price);
            return sb.ToString();
        }

        private static List<EquityQuote> Deserialize(string text)
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
    }
}