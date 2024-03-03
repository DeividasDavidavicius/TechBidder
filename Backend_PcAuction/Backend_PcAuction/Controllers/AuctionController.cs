using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/auctions")]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionsRepository _auctionsRepository;

        public AuctionController(IAuctionsRepository auctionsRepository)
        {
            _auctionsRepository = auctionsRepository;
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> Create(CreateAuctionDto createAuctionDto)
        {
            var auction = new Auction
            {
                Name = createAuctionDto.Name,
                CreationDate = DateTime.Now,
                StartDate = createAuctionDto.StartDate,
                EndDate = createAuctionDto.EndDate
            };

            await _auctionsRepository.CreateAsync(auction);

            return Created($"/api/v1/auctions/{auction.Id}", new AuctionDto(auction.Id, auction.Name, auction.CreationDate, auction.StartDate, auction.EndDate));
        }

        [HttpGet]
        [Route("{auctionId}")]
        public async Task<ActionResult<AuctionDto>> Get(Guid auctionId)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auctionId == null)
            {
                return NotFound();
            }

            return Ok(new AuctionDto(auction.Id, auction.Name, auction.CreationDate, auction.StartDate, auction.EndDate));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuctionDto>>> GetMany()
        {
            var auctions = await _auctionsRepository.GetManyAsync();
            return Ok(auctions.Select(auction => new AuctionDto(auction.Id, auction.Name, auction.CreationDate, auction.StartDate, auction.EndDate)));

        }

        [HttpPatch]
        [Route("{auctionId}")]
        public async Task<ActionResult<AuctionDto>> Update(Guid auctionId, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            auction.Name = updateAuctionDto.Name;
            await _auctionsRepository.UpdateAsync(auction);

            return Ok(new AuctionDto(auction.Id, auction.Name, auction.CreationDate, auction.StartDate, auction.EndDate));
        }

        [HttpDelete]
        [Route("{auctionId}")]
        public async Task<ActionResult> Delete(Guid auctionId)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            await _auctionsRepository.DeleteAsync(auction);

            return NoContent();
        }
    }
}
