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
            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(createAuctionDto.ImageData);
            }
            catch
            {
                imageBytes = new byte[0];
            }

            if(createAuctionDto.Name.Length < 5 || createAuctionDto.Name.Length > 50)
            {
                return UnprocessableEntity("Title must be at 5 - 50 characters long");
            }

            if (createAuctionDto.Description.Length < 10)
            {
                return UnprocessableEntity("Description must be at least 10 characters long");
            }

            if (createAuctionDto.MinIncrement < 0)
            {
                return UnprocessableEntity("Minimum increment must 0 or a positive number");
            }

            // ADD DATE VALIDATIONS

            var auction = new Auction
            {
                Name = createAuctionDto.Name,
                Description = createAuctionDto.Description,
                CreationDate = DateTime.UtcNow,
                StartDate = createAuctionDto.StartDate,
                EndDate = createAuctionDto.EndDate,
                MinIncrement = createAuctionDto.MinIncrement,
                Status = "New",
                Condition = createAuctionDto.Condition,
                Manufacturer = createAuctionDto.Manufacturer,
                Picture = createAuctionDto.Picture,
                ImageData = imageBytes,
                UserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            };

            await _auctionsRepository.CreateAsync(auction);

            return Created($"/api/v1/auctions/{auction.Id}", 
                new AuctionDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate,auction.EndDate,
                auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.UserId));
        }

        [HttpGet]
        [Route("{auctionId}")]
        public async Task<ActionResult<AuctionDtoWithImage>> Get(Guid auctionId)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var imageData = Convert.ToBase64String(auction.ImageData);

            return Ok(new AuctionDtoWithImage(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate,
                auction.EndDate, auction.MinIncrement, auction.Condition, auction.Manufacturer, imageData, auction.UserId));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuctionDto>>> GetMany()
        {
            var auctions = await _auctionsRepository.GetManyAsync();
            return Ok(auctions.Select(auction => 
            new AuctionDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate, auction.EndDate,
                auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.UserId)));

        }

        [HttpGet]
        [Route("pagination")]
        public async Task<ActionResult<AuctionsWithPaginationDto>> GetManyWithPagination(int page = 1)
        {
            var auctions = await _auctionsRepository.GetManyWithPaginationAsync(page);
            var auctionCount = await _auctionsRepository.GetCountAsync();

            var resultAuctions = auctions.Select(auction =>
            new AuctionDtoWithImage(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate, auction.EndDate,
                auction.MinIncrement, auction.Condition, auction.Manufacturer, Convert.ToBase64String(auction.ImageData), auction.UserId));

            return Ok(new AuctionsWithPaginationDto(resultAuctions, auctionCount));
        }

        [HttpPut]
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

            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(updateAuctionDto.ImageData);
            }
            catch
            {
                imageBytes = new byte[0];
            }

            if (updateAuctionDto.Name.Length < 5 || updateAuctionDto.Name.Length > 50)
            {
                return UnprocessableEntity("Title must be at 5 - 50 characters long");
            }

            if (updateAuctionDto.Description.Length < 10)
            {
                return UnprocessableEntity("Description must be at least 10 characters long");
            }

            if(updateAuctionDto.MinIncrement < 0)
            {
                return UnprocessableEntity("Minimum increment must 0 or a positive number");
            }

            // ADD DATE VALIDATIONS

            auction.Name = updateAuctionDto.Name;
            auction.Description = updateAuctionDto.Description;
            auction.StartDate = updateAuctionDto.StartDate;
            auction.EndDate = updateAuctionDto.EndDate;
            auction.MinIncrement = updateAuctionDto.MinIncrement;
            auction.Condition = updateAuctionDto.Condition;
            auction.Manufacturer = updateAuctionDto.Manufacturer;
            auction.ImageData = imageBytes;

            await _auctionsRepository.UpdateAsync(auction);

            return Ok(new AuctionDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate,
                auction.EndDate, auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.UserId));
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
