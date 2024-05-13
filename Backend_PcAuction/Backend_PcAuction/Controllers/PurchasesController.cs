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
        private readonly IPurchasesRepository _purchaseRepository;
        private readonly IAuctionsRepository _auctionsRepository;
        private readonly IStripePaymentsRepository _stripePaymentRepository;
        private readonly IPartsRepository _partsRepository;
        private readonly IPartsPricesRepository _partsPricesRepository;
        private readonly IPartPricesService _partPricesService;
        private readonly IStripeService _stripeService;

        public PurchasesController(IPurchasesRepository purchaseRepository, IAuctionsRepository auctionsRepository, IStripePaymentsRepository stripePaymentRepository,
            IPartsRepository partsRepository, IPartsPricesRepository partsPricesRepository, IPartPricesService partPricesService, IStripeService stripeService) 
        {
            _purchaseRepository = purchaseRepository;
            _auctionsRepository = auctionsRepository;
            _stripePaymentRepository = stripePaymentRepository;
            _partsRepository = partsRepository;
            _partsPricesRepository = partsPricesRepository;
            _partPricesService = partPricesService;
            _stripeService = stripeService;
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

            await Task.Delay(500); // 500ms delay just to be sure that Stripe finishes processing payment and my API can actually fetch the payment

            var stripePayment = await _stripePaymentRepository.GetLastAsync(purchase.Id);

            var paymentStatus = await _stripeService.GetPurchaseStatus(stripePayment.Id);

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
            await _partsRepository.UpdateAsync(part);

            auction.Status = AuctionStatuses.Paid;
            await _auctionsRepository.UpdateAsync(auction);

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

            var purchaseStripeDto = await _stripeService.CreateStripeSession(auction, purchase);

            var stripePayment = new StripePayment
            {
                Id = purchaseStripeDto.Id,
                PaymentDate = DateTime.UtcNow,
                Purchase = purchase
            };

            await _stripePaymentRepository.CreateAsync(stripePayment);

            return Ok(purchaseStripeDto);
        }
    }
}
