using System;
using Autofac;
using NLog;
using XPricer.Framework;
using XPricer.Injection;
using XPricer.Model;
using XPricer.Scheduler;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using XPricer.Model.Product;
using XPricer.Core;
using System.Collections.Generic;
using XPricer.Model.MarketData;
using XPricer.MarketDataGenerator;

namespace XPricer.TaskRunner
{
    class Program
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(typeof(Program));

        private const String QUOTES = "-QUOTES";
        private const String VOLATILITIES = "-VOLATILITIES";
        private const String MARKET_DATA_CONTAINER = "marketdata";

        enum ExitCode : int
        {
            Success = 0,
            InvalidArgument = 1,
            UnknownError = 10
        }

        static int Main(string[] args)
        {

            try
            {
                var container = Startup.Init();
                var loggerConfigurator = container.Resolve<ILoggerConfigurator>();
                loggerConfigurator.Configure();
                logger.Info("Starting the Task Runner");

                if (args.Length != 2)
                {
                    logger.Error("invalid number of arguments");
                    return (int)ExitCode.InvalidArgument;
                }

                var requestBlobFile = args[0];
                var containerSasURL = args[1];
                var computeRequest = ExtractComputeRequest(requestBlobFile, containerName, containerSasURL);
                var vanilla = computeRequest.Product as VanillaOption;
                var config = computeRequest.Config;

                var underlying = vanilla.Underlying;

                var quotesBlobName = String.Format("{0}{1}", underlying, QUOTES).ToUpper();
                var volatilitiesBlobName= String.Format("{0}{1}", underlying, VOLATILITIES).ToUpper();

                List<EquityQuote> quoteList = FakeMarketData.DeserializeQuotes(readBlobText(quotesBlobName, containerSasURL));
                List<EquityVolatility> volatilityList = FakeMarketData.DeserializeVolatilities(readBlobText(volatilitiesBlobName, containerSasURL));
            }
            catch (Exception e)
            {
                logger.Error("Task finished in error");
                logger.Error(e);
                return (int)ExitCode.UnknownError;

            }

            logger.Info("Task finished with success");
            return (int) ExitCode.Success;
        }

        private static ComputeRequest ExtractComputeRequest(String requestBlob, String containerName, String sasUri)
        {
            String blobText = readBlobText(requestBlob, sasUri);
            ComputeRequest crResult = JsonConvert.DeserializeObject<ComputeRequest>(blobText);

            return crResult;
        }

        private static void CalculateVanillaOption(VanillaOption option, PricingConfig config, List<EquityQuote> quote, List<EquityVolatility> volatility)
        { 
            var priceComputerFactory = new PriceComputerFactory();
            var priceComputer = (PriceComputer)priceComputerFactory.Create(config, option);

            priceComputer.EquityQuotes = quote;
            priceComputer.EquityVolatilities = volatility;
            var result = priceComputer.Compute();
        }

        private static string readBlobText(String requestBlob, String sasUri)
        {
            CloudBlobContainer container = new CloudBlobContainer(new Uri(sasUri));

            CloudBlockBlob blob = container.GetBlockBlobReference(requestBlob);

            return blob.DownloadText();
        }
    }
}
