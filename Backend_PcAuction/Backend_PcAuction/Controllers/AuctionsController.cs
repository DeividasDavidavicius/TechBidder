using Backend_PcAuction.Auth.Models;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly IAuctionsRepository _auctionsRepository;
        private readonly IAuthorizationService _authorizationService;

        public AuctionsController(IAuctionsRepository auctionsRepository, IAuthorizationService authorizationService)
        {
            _auctionsRepository = auctionsRepository;
            _authorizationService = authorizationService;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult<AuctionDto>> Create(CreateAuctionDto createAuctionDto)
        {
            var auction = new Auction
            {
                Name = createAuctionDto.Name,
                CreationDate = DateTime.Now,
                StartDate = createAuctionDto.StartDate,
                EndDate = createAuctionDto.EndDate,
                UserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            };

            await _auctionsRepository.CreateAsync(auction);

            return Created($"/api/v1/auctions/{auction.Id}", new AuctionDto(auction.Id, auction.Name, auction.CreationDate, auction.StartDate, auction.EndDate));
        }

        [HttpGet]
        [Route("{auctionId}")]
        public async Task<ActionResult<AuctionDto>> Get(Guid auctionId)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
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
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult<AuctionDto>> Update(Guid auctionId, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, auction, PolicyNames.ResourceOwner);
            if (!authorizationResult.Succeeded)
                return Forbid();

            auction.Name = updateAuctionDto.Name;
            await _auctionsRepository.UpdateAsync(auction);

            return Ok(new AuctionDto(auction.Id, auction.Name, auction.CreationDate, auction.StartDate, auction.EndDate));
        }

        [HttpDelete]
        [Route("{auctionId}")]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult> Delete(Guid auctionId)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, auction, PolicyNames.ResourceOwner);
            if (!authorizationResult.Succeeded)
                return Forbid();

            await _auctionsRepository.DeleteAsync(auction);

            return NoContent();
        }
    }
}
