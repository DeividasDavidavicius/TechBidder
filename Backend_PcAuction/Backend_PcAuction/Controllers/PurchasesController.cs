using Backend_PcAuction.Auth.Models;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/auctions/{auctionId}/purchases")]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IAuctionsRepository _auctionsRepository;
        private readonly StripeSettings _stripeSettings;

        public PurchasesController(IPurchaseRepository purchaseRepository, IAuctionsRepository auctionsRepository, IOptions<StripeSettings> stripeSettings) 
        {
            _purchaseRepository = purchaseRepository;
            _auctionsRepository = auctionsRepository;
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

            return Ok(new PurchaseDto(purchase.Id, purchase.Amount, purchase.Status, purchase.AuctionWinDate, purchase.PaymentDate, purchase.Buyer.Id, purchase.Auction.Id));
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

            var currency = "usd"; // "eur"
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
                SuccessUrl = "http://localhost:3000/",
                CancelUrl = "http://localhost:3000/",
                
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Ok(new PurchaseStripeDto(session.Id, session.Url));
        }
    }
}
