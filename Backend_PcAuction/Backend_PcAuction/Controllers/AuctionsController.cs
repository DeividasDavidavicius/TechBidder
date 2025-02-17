﻿using Backend_PcAuction.Data.Dtos;
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
        private readonly IPartCategoriesRepository _partCategoriesRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IAuctionsService _auctionService;
        private readonly IBidsRepository _bidsRepository;

        public AuctionsController(IAuctionsRepository auctionsRepository, IPartsRepository partsRepository, IPartCategoriesRepository partCategoriesRepository,
            IAuthorizationService authorizationService, IAzureBlobStorageService azureBlobStorageService, IAuctionsService auctionService, IBidsRepository bidsRepository)
        {
            _auctionsRepository = auctionsRepository;
            _partsRepository = partsRepository;
            _partCategoriesRepository = partCategoriesRepository;
            _authorizationService = authorizationService;
            _azureBlobStorageService = azureBlobStorageService;
            _auctionService = auctionService;
            _bidsRepository = bidsRepository;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult<AuctionDto>> Create([FromForm] CreateAuctionDto createAuctionDto)
        {
            if (createAuctionDto.Name.Length < 5 || createAuctionDto.Name.Length > 45)
            {
                return UnprocessableEntity("Title must be 5 - 45 characters long");
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

            var part = new Part();
            var status = AuctionStatuses.New;

            if(createAuctionDto.PartId !=  null)
            {
                part = await _partsRepository.GetAsync(createAuctionDto.PartCategory, createAuctionDto.PartId);

                if (part == null)
                {
                    return NotFound("Part not found");
                }
            }
            else if (createAuctionDto.PartName != null && createAuctionDto.PartCategoryName != null)
            {
                var category = await _partCategoriesRepository.GetAsync(createAuctionDto.PartCategoryName);

                if (category == null)
                {
                    return NotFound();
                }

                status = AuctionStatuses.NewNA;
                part = new Part { Name = createAuctionDto.Name, Type = PartTypes.Temporary, Category = category };
            }

            var imageUri = await _azureBlobStorageService.UploadImageAsync(createAuctionDto.Image);

            // TODO Create and update, backend and frontend: Max auction length 7 days (or 14), min 1 hour (or 1 day)

            var auction = new Auction
            {
                Name = createAuctionDto.Name,
                Description = createAuctionDto.Description,
                CreationDate = DateTime.UtcNow,
                StartDate = createAuctionDto.StartDate,
                EndDate = createAuctionDto.EndDate,
                MinIncrement = createAuctionDto.MinIncrement,
                Status = status,
                Condition = createAuctionDto.Condition,
                Manufacturer = createAuctionDto.Manufacturer,
                ImageUri = imageUri,
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
        public async Task<ActionResult<AuctionsWithPaginationDto>> GetManyWithPagination(int page = 1, string categoryId = null, Guid? seriesId = null, Guid? partId = null, string sortType = AuctionSortingTypes.CreationDate)
        {
            if(sortType != AuctionSortingTypes.CreationDate && sortType != AuctionSortingTypes.TimeLeft)
            {
                return UnprocessableEntity();
            }

            var auctions = await _auctionsRepository.GetManyWithPaginationAsync(page, categoryId, seriesId, partId, sortType);
            var auctionCount = await _auctionsRepository.GetCountAsync(categoryId, seriesId, partId);

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

            if (updateAuctionDto.Name.Length < 5 || updateAuctionDto.Name.Length > 45)
            {
                return UnprocessableEntity("Title must be 5 - 45 characters long");
            }

            if (updateAuctionDto.Description.Length < 10)
            {
                return UnprocessableEntity("Description must be at least 10 characters long");
            }

            if(updateAuctionDto.MinIncrement < 0)
            {
                return UnprocessableEntity("Minimum increment must 0 or a positive number");
            }

            if (auction.Status == AuctionStatuses.New || auction.Status == AuctionStatuses.NewNA)
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


            var highestBid = await _bidsRepository.GetLastAsync(auction.Id);
            if (!(auction.Status == "New" || auction.Status == "NewNA" || (auction.Status == "Active" || auction.Status == "ActiveNA") && highestBid == null))
            {
                return UnprocessableEntity("Can not edit auctions with bids");
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

        [HttpPatch]
        [Route("{auctionId}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<AuctionDto>> UpdateAuctionsPart(Guid auctionId, UpdateAuctionPartDto? updateAuctionPartDto)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, auction, PolicyNames.ResourceOwner);
            if (!authorizationResult.Succeeded)
                return Forbid();

            if (updateAuctionPartDto != null)
            {
                var part = await _partsRepository.GetAsync(updateAuctionPartDto.CategoryId, updateAuctionPartDto.PartId);

                if (part == null)
                {
                    return NotFound();
                }

                auction.Part = part;
            }

            if (auction.Status == AuctionStatuses.NewNA)
                auction.Status = AuctionStatuses.New;
            if (auction.Status == AuctionStatuses.ActiveNA)
                auction.Status = AuctionStatuses.Active;

            await _auctionsRepository.UpdateAsync(auction);

            return Ok(new AuctionDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate,
                auction.EndDate, auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.ImageUri, auction.Status,
                auction.UserId, auction.Part.Id, auction.Part.Category.Id));
        }

        [HttpPatch]
        [Route("{auctionId}/cancel")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<AuctionDto>> CancelAuction(Guid auctionId)
        {
            var auction = await _auctionsRepository.GetAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var highestBid = await _bidsRepository.GetLastAsync(auction.Id);
            if (!(auction.Status == "New" || auction.Status == "NewNA" || (auction.Status == "Active" || auction.Status == "ActiveNA") && highestBid == null))
            {
                return UnprocessableEntity("Can not cancel auctions with bids");
            }

            auction.Status = AuctionStatuses.Cancelled;
            auction.EndDate = DateTime.UtcNow;
            await _auctionsRepository.UpdateAsync(auction);

            return Ok(new AuctionDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate,
                auction.EndDate, auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.ImageUri, auction.Status,
                auction.UserId, auction.Part.Id, auction.Part.Category.Id));
        }

        [HttpGet]
        [Route("{auctionId}/recommendations")]
        public async Task<ActionResult<IEnumerable<AuctionWithPartNameDto>>>GetRecommendations(Guid auctionId)
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
