using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppInsightData
{
    public class WebRequestSimulator
    {
        private readonly ILogger _logger;
        public EventHandler<string> ProductRequestSent;
        public EventHandler<string> ProductTypeRequestSent;
        public WebRequestSimulator(ILogger logger)
        {
            _logger = logger;
        }

        public async Task StartGeneratingProductUrls(int[] productIDs, int requestsPerSecond = 5,  CancellationToken token = default(CancellationToken))
        {
            Random rnd = new Random();
            int delayTimeInMS = 1000 / requestsPerSecond;

            while (true)
            {
                int productID = rnd.Next(0, productIDs.Length);
                string data = $"GET https://www.shop.acme/products/{productID}";
                _logger.LogInformation(data);
                ProductRequestSent?.Invoke(this, data);
                await Task.Delay(delayTimeInMS);
                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
        }
        public async Task StartGeneratingProductTypeUrls(int[] productTypeIDs, int requestsPerSecond = 1, CancellationToken token = default(CancellationToken))
        {
            Random rnd = new Random();
            int delayTimeInMS = 1000 / requestsPerSecond;

            while (true)
            {
                int productTypeID = rnd.Next(0, productTypeIDs.Length);
                string data = $"GET https://www.shop.acme/category/{productTypeID}";
                _logger.LogInformation(data);
                ProductTypeRequestSent?.Invoke(this, data);
                await Task.Delay(delayTimeInMS);
                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }
}
