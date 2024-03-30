using Backend_PcAuction.Auth.Models;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/auctions/{auctionId}/bids")]
    public class BidsController : ControllerBase
    {
        private readonly IAuctionsRepository _auctionsRepository;
        private readonly IBidsRepository _bidsRepository;
        private readonly IAuthorizationService _authorizationService;

        public BidsController(IAuctionsRepository auctionsRepository, IBidsRepository bidsRepository, IAuthorizationService authorizationService)
        {
            _auctionsRepository = auctionsRepository;
            _bidsRepository = bidsRepository;
            _authorizationService = authorizationService;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult<BidDto>> Create(Guid auctionId, CreateBidDto createBidDto)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if(auction.Status != AuctionStatuses.Active && auction.Status != AuctionStatuses.ActiveNA)
            {
                return UnprocessableEntity("Auction is not active");
            }

            if (auction == null)
            {
                return NotFound();
            }

            // TODO - Padaryt, kad neitu same useriui keliu bid is eiles
            var lastBid = await _bidsRepository.GetLastAsync(auctionId);
            if (lastBid != null)
            {
                if (auction.MinIncrement > 0 && createBidDto.Amount < lastBid.Amount + auction.MinIncrement)
                {
                    return UnprocessableEntity("Your bid is not higher by min. increment than previous bid");
                }

                if (auction.MinIncrement == 0 && createBidDto.Amount <= lastBid.Amount)
                {
                    return UnprocessableEntity("Your bid is not higher than the previous bid");
                }
            }
            else if (auction.MinIncrement > 0 && createBidDto.Amount < auction.MinIncrement)
            {
                return UnprocessableEntity("Starting bid must be equal or higher than min. increment");
            }
            else if (auction.MinIncrement == 0 && createBidDto.Amount <= 0)
            {
                return UnprocessableEntity("Starting bid must be a positive number");
            }

            if(createBidDto.Amount > 50000)
            {
                return UnprocessableEntity("Bid can not exceed 50000€");
            }

            if(DateTime.UtcNow > auction.EndDate)
            {
                return UnprocessableEntity("You can not bid because auction ended");
            }

            var bid = new Bid
            {
                Amount = createBidDto.Amount,
                CreationDate = DateTime.UtcNow,
                Auction = auction,
                UserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            };

            await _bidsRepository.CreateAsync(bid);

            auction.HighestBid = bid.Amount;
            _auctionsRepository.UpdateAsync(auction);

            return Created($"/api/v1/auctions/{auctionId}/bids/{bid.Id}", 
                new BidDto(bid.Id, bid.Amount, bid.CreationDate));
        }

        [HttpGet]
        [Route("{bidId}")]
        public async Task<ActionResult<BidDto>> Get(Guid auctionId, Guid bidId)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var bid = await _bidsRepository.GetAsync(auctionId, bidId);

            if (bid == null)
            {
                return NotFound();
            }

            return Ok(new BidDto(bid.Id, bid.Amount, bid.CreationDate));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bid>>> GetMany(Guid auctionId)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var bids = await _bidsRepository.GetManyAsync(auctionId);

            return Ok(bids.Select(bid => new BidDto(bid.Id, bid.Amount, bid.CreationDate)));
        }

        [HttpDelete]
        [Route("{bidId}")]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult> Delete(Guid auctionId, Guid bidId)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var bid = await _bidsRepository.GetAsync(auctionId, bidId);

            if (bid == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, bid, PolicyNames.ResourceOwner);
            if (!authorizationResult.Succeeded)
                return Forbid();

            await _bidsRepository.DeleteAsync(bid);

            return NoContent();
        }

        [HttpGet]
        [Route("highest")]
        public async Task<ActionResult<BidDto>> GetHighest(Guid auctionId)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var bid = await _bidsRepository.GetLastAsync(auctionId);

            if (bid == null)
            {
                return Ok(new BidDto(new Guid(), -1, DateTime.UtcNow));
            }

            return Ok(new BidDto(bid.Id, bid.Amount, bid.CreationDate));
        }
    }
}
