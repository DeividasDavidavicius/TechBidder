using Backend_PcAuction.Auth.Model;
using Backend_PcAuction.Auth.Models;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/categories/{categoryId}/parts")]
    public class PartsController : ControllerBase
    {
        private readonly IPartCategoriesRepository _partCategoriesRepository;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IPartsRepository _partsRepository;

        public PartsController(IPartCategoriesRepository partCategoriesRepository, ISeriesRepository seriesRepository, IPartsRepository partsRepository)
        {
            _partCategoriesRepository = partCategoriesRepository;
            _seriesRepository = seriesRepository;
            _partsRepository = partsRepository;
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

            var series = await _seriesRepository.GetAsync(categoryId, createPartDto.SeriesId);

            if (createPartDto.SeriesId != null && series == null)
            {
                return NotFound();
            }

            var part = new Part
            {
                Name = createPartDto.Name,
                Type = PartTypes.Permanent,
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
                AveragePrice = -1,
                Category = category,
                Series = series,
            };

            await _partsRepository.CreateAsync(part);

            return Created($"/api/v1/categories/{categoryId}/parts/{part.Id}",
                new PartDto(part.Id, part.Name, part.SpecificationValue1, part.SpecificationValue2, part.SpecificationValue3,
                 part.SpecificationValue4, part.SpecificationValue5, part.SpecificationValue6, part.SpecificationValue7,
                 part.SpecificationValue8, part.SpecificationValue9, part.SpecificationValue10, part.Category.Id, part.Type, 
                 part.AveragePrice, part.Series?.Id));
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
                 part.SpecificationValue8, part.SpecificationValue9, part.SpecificationValue10, part.Category.Id, part.Type,
                 part.AveragePrice, part.Series?.Id));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PartDto>>> GetMany(string categoryId)
        {
            var category = await _partCategoriesRepository.GetAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            var parts = await _partsRepository.GetManyAsync(categoryId);

            return Ok(parts.Select(part => new PartDto(part.Id, part.Name, part.SpecificationValue1, part.SpecificationValue2, part.SpecificationValue3,
                 part.SpecificationValue4, part.SpecificationValue5, part.SpecificationValue6, part.SpecificationValue7,
                 part.SpecificationValue8, part.SpecificationValue9, part.SpecificationValue10, part.Category.Id, part.Type,
                 part.AveragePrice, part.Series?.Id)));
        }

        [HttpGet]
        [Route("requests")]
        public async Task<ActionResult<IEnumerable<PartDto>>> GetManyTemp(string categoryId)
        {
            var category = await _partCategoriesRepository.GetAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            var parts = await _partsRepository.GetManyTempAsync(categoryId);

            return Ok(parts.Select(part => new PartDto(part.Id, part.Name, part.SpecificationValue1, part.SpecificationValue2, part.SpecificationValue3,
                 part.SpecificationValue4, part.SpecificationValue5, part.SpecificationValue6, part.SpecificationValue7,
                 part.SpecificationValue8, part.SpecificationValue9, part.SpecificationValue10, part.Category.Id, part.Type,
                 part.AveragePrice, part.Series?.Id)));
        }

        [HttpPatch]
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

            var series = await _seriesRepository.GetAsync(categoryId, updatePartDto.SeriesId);

            if (updatePartDto.SeriesId != null && series == null)
            {
                return NotFound();
            }

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
            part.Series = series;
            part.Type = updatePartDto.Type;

            await _partsRepository.UpdateAsync(part);

            return Ok(new PartDto(part.Id, part.Name, part.SpecificationValue1, part.SpecificationValue2, part.SpecificationValue3,
                 part.SpecificationValue4, part.SpecificationValue5, part.SpecificationValue6, part.SpecificationValue7,
                 part.SpecificationValue8, part.SpecificationValue9, part.SpecificationValue10, part.Category.Id, part.Type,
                 part.AveragePrice, part.Series?.Id));
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
