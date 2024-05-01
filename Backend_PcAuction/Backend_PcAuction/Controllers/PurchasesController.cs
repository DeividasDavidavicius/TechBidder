using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Services;
using Backend_PcAuction.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/auctions/{auctionId}/purchases")]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IAuctionsRepository _auctionsRepository;
        private readonly IStripePaymentRepository _stripePaymentRepository;
        private readonly IPartsRepository _partsRepository;
        private readonly IPartsPricesRepository _partsPricesRepository;
        private readonly IPartPricesService _partPricesService;
        private readonly StripeSettings _stripeSettings;

        public PurchasesController(IPurchaseRepository purchaseRepository, IAuctionsRepository auctionsRepository, IStripePaymentRepository stripePaymentRepository,
            IPartsRepository partsRepository, IPartsPricesRepository partsPricesRepository, IPartPricesService partPricesService, IOptions<StripeSettings> stripeSettings) 
        {
            _purchaseRepository = purchaseRepository;
            _auctionsRepository = auctionsRepository;
            _stripePaymentRepository = stripePaymentRepository;
            _partsRepository = partsRepository;
            _partsPricesRepository = partsPricesRepository;
            _partPricesService = partPricesService;
            _stripeSettings = stripeSettings.Value;
        }

        [HttpGet]
        public async Task<ActionResult<PurchaseDto>> Get(Guid auctionId)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var purchase = await _purchaseRepository.GetLastAsync(auctionId);

            if (purchase == null)
            {
                return NotFound();
            }

            return Ok(new PurchaseDto(purchase.Id, purchase.Amount, purchase.Status, purchase.AuctionWinDate, purchase.Buyer.Id, purchase.Auction.Id));
        }

        [HttpPatch]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult<SeriesDto>> Update(Guid auctionId)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var purchase = await _purchaseRepository.GetLastAsync(auctionId);

            if (purchase == null)
            {
                return NotFound();
            }

            if(purchase.Status != PurchaseStatuses.PendingPayment)
            {
                return Forbid();
            }

            string paymentStatus = "unpaid";

            await Task.Delay(250); // 250ms delay just to be sure that Stripe finishes processing payment and my API can actually fetch the payment

            var stripePayment = await _stripePaymentRepository.GetLastAsync(purchase.Id);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _stripeSettings.SecretKey);

                try
                {
                    HttpResponseMessage response = await client.GetAsync($"https://api.stripe.com/v1/checkout/sessions/{stripePayment.Id}");
                    
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

            if(paymentStatus != "paid")
            {
                return Forbid();
            }

            purchase.Status = PurchaseStatuses.Paid;
            await _purchaseRepository.UpdateAsync(purchase);

            var part = auction.Part;

            var partPrice = new PartPrice
            {
                Type = PartPriceTypes.PartAuctionSell,
                Price = purchase.Amount,
                PriceCheckDate = DateTime.UtcNow,
                Part = part
            };
            await _partsPricesRepository.CreateAsync(partPrice);

            part.AveragePrice = await _partPricesService.GetPriceAverageAsync(part.Id);
            _partsRepository.UpdateAsync(part);

            auction.Status = AuctionStatuses.Paid;
            _auctionsRepository.UpdateAsync(auction);

            return Ok(new PurchaseDto(purchase.Id, purchase.Amount, purchase.Status, purchase.AuctionWinDate, purchase.Buyer.Id, purchase.Auction.Id));
        }

        [HttpGet]
        [Route("stripe")]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult<PurchaseStripeDto>> PostStripe(Guid auctionId)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var purchase = await _purchaseRepository.GetLastAsync(auctionId);

            if (purchase == null)
            {
                return NotFound();
            }

            if (User.FindFirstValue(JwtRegisteredClaimNames.Sub) != purchase.BuyerId)
            {
                return Forbid();
            }

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
                SuccessUrl = $"http://localhost:3000/auctions/{auctionId}?status=success",
                CancelUrl = $"http://localhost:3000/auctions/{auctionId}"
            };

            var service = new SessionService();
            var session = service.Create(options);

            var stripePayment = new StripePayment
            {
                Id = session.Id,
                PaymentDate = DateTime.UtcNow,
                Purchase = purchase
            };

            await _stripePaymentRepository.CreateAsync(stripePayment);

            return Ok(new PurchaseStripeDto(session.Id, session.Url));
        }
    }
}
