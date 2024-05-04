using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Utils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Stripe.Checkout;
using Stripe;
using System.Net.Http.Headers;

namespace Backend_PcAuction.Services
{
    public interface IStripeService
    {
        Task<string> GetPurchaseStatus(string paymentId);
        Task<PurchaseStripeDto> CreateStripeSession(Auction auction, Purchase purchase);
    }

    public class StripeService : IStripeService
    {
        private readonly StripeSettings _stripeSettings;

        public StripeService(IOptions<StripeSettings> stripeSettings)
        {
            _stripeSettings = stripeSettings.Value;
        }

        public async Task<string> GetPurchaseStatus(string paymentId)
        {
            string paymentStatus = "unpaid";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _stripeSettings.SecretKey);

                try
                {
                    HttpResponseMessage response = await client.GetAsync($"https://api.stripe.com/v1/checkout/sessions/{paymentId}");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        dynamic responseObject = JsonConvert.DeserializeObject(responseBody);
                        paymentStatus = responseObject.payment_status;
                    }
                }
                catch (Exception ex)
                {
                }
            }

            return paymentStatus;
        }

        public async Task<PurchaseStripeDto> CreateStripeSession(Auction auction, Purchase purchase)
        {
            var currency = "eur";
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = currency,
                            UnitAmount = Convert.ToInt32(purchase.Amount) * 100,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = auction.Part.Name,
                                Description = auction.Name
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"http://localhost:3000/auctions/{auction.Id}?status=success",
                CancelUrl = $"http://localhost:3000/auctions/{auction.Id}"
            };

            var service = new SessionService();
            var session = service.Create(options);

            return new PurchaseStripeDto(session.Id, session.Url);
        }
    }
}
