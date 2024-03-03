using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/auctions/{auctionId}/bids")]
    public class BidsController : ControllerBase
    {
        private readonly IAuctionsRepository _auctionsRepository;
        private readonly IBidsRepository _bidsRepository;

        public BidsController(IAuctionsRepository auctionsRepository, IBidsRepository bidsRepository)
        {
            _auctionsRepository = auctionsRepository;
            _bidsRepository = bidsRepository;
        }

        [HttpPost]
        public async Task<ActionResult<BidDto>> Create(Guid auctionId, CreateBidDto createBidDto)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var bid = new Bid
            {
                Amount = createBidDto.Amount,
                CreationDate = DateTime.Now,
                Auction = auction
            };

            await _bidsRepository.CreateAsync(bid);

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
            Console.WriteLine(auctionId + " " + bidId);

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

            await _bidsRepository.DeleteAsync(bid);

            return NoContent();
        }
    }
}
