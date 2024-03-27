using Backend_PcAuction.Auth.Models;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Services;
using Backend_PcAuction.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly IAuctionsRepository _auctionsRepository;
        private readonly IPartsRepository _partsRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IAuctionService _auctionService;


        public AuctionsController(IAuctionsRepository auctionsRepository, IPartsRepository partsRepository,IAuthorizationService authorizationService,
            IAzureBlobStorageService azureBlobStorageService, IAuctionService auctionService)
        {
            _auctionsRepository = auctionsRepository;
            _partsRepository = partsRepository;
            _authorizationService = authorizationService;
            _azureBlobStorageService = azureBlobStorageService;
            _auctionService = auctionService;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult<AuctionDto>> Create([FromForm] CreateAuctionDto createAuctionDto)
        {
            var imageUri = await _azureBlobStorageService.UploadImageAsync(createAuctionDto.Image);

            if (createAuctionDto.Name.Length < 5 || createAuctionDto.Name.Length > 30)
            {
                return UnprocessableEntity("Title must be at 5 - 30 characters long");
            }

            if (createAuctionDto.Description.Length < 10)
            {
                return UnprocessableEntity("Description must be at least 10 characters long");
            }

            if (createAuctionDto.MinIncrement < 0)
            {
                return UnprocessableEntity("Minimum increment must 0 or a positive number");
            }

            if (createAuctionDto.StartDate < DateTime.UtcNow)
            {
                return UnprocessableEntity("Start date must be later than current time");
            }

            if (createAuctionDto.EndDate < DateTime.UtcNow)
            {
                return UnprocessableEntity("End date must be later than current time");
            }

            if (createAuctionDto.EndDate < createAuctionDto.StartDate)
            {
                return UnprocessableEntity("End date must be later than start date");
            }

            var part = await _partsRepository.GetAsync(createAuctionDto.PartCategory, createAuctionDto.PartId);

            if (part == null)
            {
                return NotFound("Part not found");
            }

            // TODO Create and update, backend and frontend: Max auction length 7 days (or 14), min 1 hour (or 1 day)

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
                ImageUri = imageUri,
                HighestBid = -1,
                UserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub),
                Part = part
            };

            await _auctionsRepository.CreateAsync(auction);

            return Created($"/api/v1/auctions/{auction.Id}", 
                new AuctionDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate,auction.EndDate,
                auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.ImageUri, auction.Status, auction.UserId,
                auction.Part.Id, auction.Part.Category.Id));
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

            return Ok(new AuctionDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate,
                auction.EndDate, auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.ImageUri, auction.Status,
                auction.UserId, auction.Part.Id, auction.Part.Category.Id));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuctionDto>>> GetMany()
        {
            var auctions = await _auctionsRepository.GetManyAsync();
            return Ok(auctions.Select(auction => 
            new AuctionDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate, auction.EndDate,
                auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.ImageUri, auction.Status,
                auction.UserId, auction.Part.Id, auction.Part.Category.Id)));

        }

        [HttpGet]
        [Route("pagination")]
        public async Task<ActionResult<AuctionsWithPaginationDto>> GetManyWithPagination(int page = 1)
        {
            var auctions = await _auctionsRepository.GetManyWithPaginationAsync(page);
            var auctionCount = await _auctionsRepository.GetCountAsync();

            var resultAuctions = auctions.Select(auction =>
            new AuctionWithPartNameDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate, auction.EndDate,
                auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.ImageUri, auction.Status, auction.UserId, auction.Part.Name,
                auction.Part.Category.Id));

            return Ok(new AuctionsWithPaginationDto(resultAuctions, auctionCount));
        }

        [HttpPut]
        [Route("{auctionId}")]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult<AuctionDto>> Update(Guid auctionId, [FromForm] UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, auction, PolicyNames.ResourceOwner);
            if (!authorizationResult.Succeeded)
                return Forbid();

            if (updateAuctionDto.Name.Length < 5 || updateAuctionDto.Name.Length > 30)
            {
                return UnprocessableEntity("Title must be at 5 - 30 characters long");
            }

            if (updateAuctionDto.Description.Length < 10)
            {
                return UnprocessableEntity("Description must be at least 10 characters long");
            }

            if(updateAuctionDto.MinIncrement < 0)
            {
                return UnprocessableEntity("Minimum increment must 0 or a positive number");
            }

            if (auction.Status == AuctionStatuses.New)
            {
                if (updateAuctionDto.StartDate < DateTime.UtcNow)
                {
                    return UnprocessableEntity("Start date must be later than current time");
                }
            }

            if (updateAuctionDto.EndDate < DateTime.UtcNow)
            {
                return UnprocessableEntity("End date must be later than current time");
            }

            if (updateAuctionDto.EndDate < updateAuctionDto.StartDate)
            {
                return UnprocessableEntity("End date must be later than start date");
            }

            if (updateAuctionDto.Image != null)
            {
                if (!auction.ImageUri.StartsWith("/default"))
                {
                    await _azureBlobStorageService.DeleteImageAsync(auction.ImageUri);
                }

                auction.ImageUri = await _azureBlobStorageService.UploadImageAsync(updateAuctionDto.Image);
            }

            auction.Name = updateAuctionDto.Name;
            auction.Description = updateAuctionDto.Description;
            auction.StartDate = updateAuctionDto.StartDate;
            auction.EndDate = updateAuctionDto.EndDate;
            auction.MinIncrement = updateAuctionDto.MinIncrement;
            auction.Condition = updateAuctionDto.Condition;
            auction.Manufacturer = updateAuctionDto.Manufacturer;

            await _auctionsRepository.UpdateAsync(auction);

            return Ok(new AuctionDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate,
                auction.EndDate, auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.ImageUri, auction.Status,
                auction.UserId, auction.Part.Id, auction.Part.Category.Id));
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

            await _azureBlobStorageService.DeleteImageAsync(auction.ImageUri);

            await _auctionsRepository.DeleteAsync(auction);

            return NoContent();
        }

        [HttpGet]
        [Route("{auctionId}/recommendations")]
        public async Task<ActionResult<IEnumerable<AuctionDto>>>GetRecommendations(Guid auctionId)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var recommendations = await _auctionService.GenerateAuctionRecommendations(auction);

            var resultAuctions = recommendations.Select(auction =>
                new AuctionWithPartNameDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate, auction.EndDate,
                    auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.ImageUri, auction.Status, auction.UserId, auction.Part.Name, auction.Part.Category.Id));

            return Ok(resultAuctions);
        }
    }
}
