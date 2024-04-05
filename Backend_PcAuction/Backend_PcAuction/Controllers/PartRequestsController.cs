using Backend_PcAuction.Auth.Models;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/partrequests")]
    public class PartRequestsController : ControllerBase
    {
        private readonly IPartRequestsRepository _partRequestsRepository;
        private readonly IPartCategoriesRepository _partCategoriesRepository;
        private readonly IPartsRepository _partsRepository;
        private readonly IAuctionsRepository _auctionsRepository;

        public PartRequestsController(IPartRequestsRepository partRequestsRepository, IPartCategoriesRepository partCategoriesRepository, IPartsRepository partsRepository, IAuctionsRepository auctionsRepository)
        {
            _partRequestsRepository = partRequestsRepository;
            _partCategoriesRepository = partCategoriesRepository;
            _partsRepository = partsRepository;
            _auctionsRepository = auctionsRepository;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.RegisteredUser)]
        public async Task<ActionResult<PartRequestDto>> Create(CreatePartRequestDto createPartRequestDto)
        {
            var category = await _partCategoriesRepository.GetAsync(createPartRequestDto.CategoryId);

            if (category == null)
            {
                return NotFound();
            }

            var part = await _partsRepository.GetAsync(category.Id, createPartRequestDto.PartId);

            if(part == null)
            {
                return NotFound();
            }

            var auction = await _auctionsRepository.GetAsync(createPartRequestDto.AuctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var partRequest = new PartRequest
            {
                Name = createPartRequestDto.Name,
                Part = part,
                Auction = auction
            };

            await _partRequestsRepository.CreateAsync(partRequest);

            return Created($"/api/v1/partrequests/{partRequest.Id}",
                new PartRequestDto(partRequest.Id, partRequest.Name, partRequest.Auction.Id, partRequest.Part.Id, partRequest.Part.Category.Id));
        }

        [HttpGet]
        [Route("{partRequestId}")]
        [Authorize(Roles = UserRoles.Admin)] 
        public async Task<ActionResult<PartRequestDto>> Get(Guid partRequestId)
        {
            var partRequest = await _partRequestsRepository.GetAsync(partRequestId);

            if (partRequest == null)
            {
                return NotFound();
            }

            return Ok(new PartRequestDto(partRequest.Id, partRequest.Name, partRequest.Auction.Id, partRequest.Part.Id, partRequest.Part.Category.Id));
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<IEnumerable<PartRequestDto>>> GetMany()
        {
            var partRequests = await _partRequestsRepository.GetManyAsync();

            return Ok(partRequests.Select(partRequest => new PartRequestDto(partRequest.Id, partRequest.Name, partRequest.Auction.Id, partRequest.Part.Id, partRequest.Part.Category.Id)));
        }

        [HttpDelete]
        [Route("{partRequestId}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult> Delete(Guid partRequestId)
        {
            var partRequest = await _partRequestsRepository.GetAsync(partRequestId);

            if (partRequest == null)
            {
                return NotFound();
            }

            await _partRequestsRepository.DeleteAsync(partRequest);

            return NoContent();
        }
    }
}
