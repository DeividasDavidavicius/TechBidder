using Backend_PcAuction.Auth.Model;
using Backend_PcAuction.Auth.Models;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/categories/{categoryId}/parts")]
    public class PartsController : ControllerBase
    {
        private readonly IPartCategoriesRepository _partCategoriesRepository;
        private readonly IPartsRepository _partsRepository;
        private readonly IAuthorizationService _authorizationService;

        public PartsController(IPartCategoriesRepository partCategoriesRepository, IPartsRepository partsRepository, IAuthorizationService authorizationService)
        {
            _partCategoriesRepository = partCategoriesRepository;
            _partsRepository = partsRepository;
            _authorizationService = authorizationService;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<PartDto>> Create(string categoryId, CreatePartDto createPartDto)
        {
            var category = await _partCategoriesRepository.GetAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            var part = new Part
            {
                Name = createPartDto.Name,
                SpecificationValue1 = createPartDto.SpecificationValue1,
                SpecificationValue2 = createPartDto.SpecificationValue2,
                SpecificationValue3 = createPartDto.SpecificationValue3,
                SpecificationValue4 = createPartDto.SpecificationValue4,
                SpecificationValue5 = createPartDto.SpecificationValue5,
                SpecificationValue6 = createPartDto.SpecificationValue6,
                SpecificationValue7 = createPartDto.SpecificationValue7,
                SpecificationValue8 = createPartDto.SpecificationValue8,
                SpecificationValue9 = createPartDto.SpecificationValue9,
                SpecificationValue10 = createPartDto.SpecificationValue10,
                Category = category,
                Series = null,
            };

            await _partsRepository.CreateAsync(part);

            return Created($"/api/v1/categories/{categoryId}/parts/{part.Id}",
                new PartDto(part.Id, part.Name, part.SpecificationValue1, part.SpecificationValue2, part.SpecificationValue3,
                 part.SpecificationValue4, part.SpecificationValue5, part.SpecificationValue6, part.SpecificationValue7,
                 part.SpecificationValue8, part.SpecificationValue9, part.SpecificationValue10, part.Category.Id, part.Series?.Id));
        }

        [HttpGet]
        [Route("{partId}")]
        public async Task<ActionResult<PartDto>> Get(string categoryId, Guid partId)
        {
            var category = await _partCategoriesRepository.GetAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            var part = await _partsRepository.GetAsync(categoryId, partId);

            if (part == null)
            {
                return NotFound();
            }

            return Ok(new PartDto(part.Id, part.Name, part.SpecificationValue1, part.SpecificationValue2, part.SpecificationValue3,
                 part.SpecificationValue4, part.SpecificationValue5, part.SpecificationValue6, part.SpecificationValue7,
                 part.SpecificationValue8, part.SpecificationValue9, part.SpecificationValue10, part.Category.Id, part.Series?.Id));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Part>>> GetMany(string categoryId)
        {
            var category = await _partCategoriesRepository.GetAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            var parts = await _partsRepository.GetManyAsync(categoryId);

            return Ok(parts.Select(part => new PartDto(part.Id, part.Name, part.SpecificationValue1, part.SpecificationValue2, part.SpecificationValue3,
                 part.SpecificationValue4, part.SpecificationValue5, part.SpecificationValue6, part.SpecificationValue7,
                 part.SpecificationValue8, part.SpecificationValue9, part.SpecificationValue10, part.Category.Id, part.Series?.Id)));
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("{partId}")]
        public async Task<ActionResult<PartDto>> Update(string categoryId, Guid partId, UpdatePartDto updatePartDto)
        {
            var category = await _partCategoriesRepository.GetAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            var part = await _partsRepository.GetAsync(categoryId, partId);

            if (part == null)
            {
                return NotFound();
            }

            Console.WriteLine(updatePartDto);

            part.Name = updatePartDto.Name;
            part.SpecificationValue1 = updatePartDto.SpecificationValue1;
            part.SpecificationValue2 = updatePartDto.SpecificationValue2;
            part.SpecificationValue3 = updatePartDto.SpecificationValue3;
            part.SpecificationValue4 = updatePartDto.SpecificationValue4;
            part.SpecificationValue5 = updatePartDto.SpecificationValue5;
            part.SpecificationValue6 = updatePartDto.SpecificationValue6;
            part.SpecificationValue7 = updatePartDto.SpecificationValue7;
            part.SpecificationValue8 = updatePartDto.SpecificationValue8;
            part.SpecificationValue9 = updatePartDto.SpecificationValue9;
            part.SpecificationValue10 = updatePartDto.SpecificationValue10;

            await _partsRepository.UpdateAsync(part);

            return Ok(new PartDto(part.Id, part.Name, part.SpecificationValue1, part.SpecificationValue2, part.SpecificationValue3,
                 part.SpecificationValue4, part.SpecificationValue5, part.SpecificationValue6, part.SpecificationValue7,
                 part.SpecificationValue8, part.SpecificationValue9, part.SpecificationValue10, part.Category.Id, part.Series?.Id));
        }

        [HttpDelete]
        [Route("{partId}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult> Delete(string categoryId, Guid partId)
        {
            var category = await _partCategoriesRepository.GetAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            var part = await _partsRepository.GetAsync(categoryId, partId);

            if (part == null)
            {
                return NotFound();
            }

            await _partsRepository.DeleteAsync(part);

            return NoContent();
        }
    }
}
