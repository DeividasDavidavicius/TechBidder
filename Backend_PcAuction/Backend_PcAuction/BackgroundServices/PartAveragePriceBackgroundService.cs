using Backend_PcAuction.Data;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Services;
using Backend_PcAuction.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace Backend_PcAuction.BackgroundServices
{
    public class PartAveragePriceBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TimeSpan _interval;

        public PartAveragePriceBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _interval = TimeSpan.FromMinutes(1);
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await CheckParts();
                await Task.Delay(_interval, token);
            }
        }

        private async Task CheckParts()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PcAuctionDbContext>();
                var partPriceService = scope.ServiceProvider.GetRequiredService<IPartPricesService>();

                var partsWithoutPrices = await dbContext.Parts.Where(p => p.Type == PartTypes.Permanent && p.AveragePrice == -1).ToListAsync();

                foreach(var part in partsWithoutPrices)
                {
                    var ebayPrice = await dbContext.PartPrices.OrderByDescending(partPrice => partPrice.PriceCheckDate).FirstOrDefaultAsync(partPrice => partPrice.Part.Id == part.Id && partPrice.Type == PartPriceTypes.EbayAverage);
                    if(ebayPrice == null)
                    {
                        double ebayMedianPrice = -10;
                        try
                        {
                            ebayMedianPrice = await GetEbayPrice(part.Name);
                        }
                        catch
                        {
                        }

                        var partPrice = new PartPrice
                        {
                            Type = PartPriceTypes.EbayAverage,
                            Price = ebayMedianPrice,
                            PriceCheckDate = DateTime.UtcNow,
                            Part = part
                        };

                        dbContext.PartPrices.Add(partPrice);

                        var logAvg = new Log
                        {
                            Message = String.Format("Added ebay part price: Time: {0}, part: {1}, price: {2}", DateTime.UtcNow, part.Name, ebayMedianPrice)
                        };
                        dbContext.Logs.Add(logAvg);
                        await dbContext.SaveChangesAsync();
                    }

                    part.AveragePrice = await partPriceService.GetPriceAverageAsync(part.Id);
                    dbContext.Parts.Update(part);

                    var log = new Log
                    {
                        Message = String.Format("Updated part price: Time: {0}, part: {1}", DateTime.UtcNow, part.Name)
                    };
                    dbContext.Logs.Add(log);
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private async Task<double> GetEbayPrice(string partName)
        {

            string contentString = $"{{\r\"keywords\": \"{partName}\",\r\"excluded_keywords\": \"broken\",\r\"max_search_results\": \"100\",\r\"max_pages\": \"5\",\r\"remove_outliers\": \"true\",\r\"site_id\": \"0\"\r}}";
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://ebay-average-selling-price.p.rapidapi.com/findCompletedItems"),
                Headers =
                {
                    { "X-RapidAPI-Key", "e5c2ad60d2msh60da4775f7b3009p1af7dbjsn2d179823caa8" },
                    { "X-RapidAPI-Host", "ebay-average-selling-price.p.rapidapi.com" },
                },
                Content = new StringContent(contentString)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                var json = JObject.Parse(body);

                double averagePrice = json["average_price"].Value<double>();
                double medianPrice = json["median_price"].Value<double>();

                return medianPrice;
            }
        }
    }
}
