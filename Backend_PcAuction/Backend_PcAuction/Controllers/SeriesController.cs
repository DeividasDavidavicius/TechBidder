using Backend_PcAuction.Auth.Models;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/categories/{categoryId}/series")]
    public class SeriesController : ControllerBase
    {
        private readonly IPartCategoriesRepository _partCategoriesRepository;
        private readonly ISeriesRepository _seriesRepository;

        public SeriesController(IPartCategoriesRepository partCategoriesRepository, ISeriesRepository seriesRepository)
        {
            _partCategoriesRepository = partCategoriesRepository;
            _seriesRepository = seriesRepository;
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<SeriesDto>> Create(string categoryId, CreateSeriesDto createSeriesDto)
        {
            var category = await _partCategoriesRepository.GetAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            var series = new Series
            {
                Name = createSeriesDto.Name,
                PartCategory = category,
            };

            await _seriesRepository.CreateAsync(series);

            return Created($"/api/v1/categories/{categoryId}/series/{series.Id}",
                new SeriesDto(series.Id, series.Name, series.PartCategory.Id));
        }

        [HttpGet]
        [Route("{seriesId}")]
        public async Task<ActionResult<SeriesDto>> Get(string categoryId, Guid seriesId)
        {
            var category = await _partCategoriesRepository.GetAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            var series = await _seriesRepository.GetAsync(categoryId, seriesId);

            if (series == null)
            {
                return NotFound();
            }

            return Ok(new SeriesDto(series.Id, series.Name, series.PartCategory.Id));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SeriesDto>>> GetMany(string categoryId)
        {
            var category = await _partCategoriesRepository.GetAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            var categorySeries = await _seriesRepository.GetManyAsync(categoryId);

            return Ok(categorySeries.Select(series => new SeriesDto(series.Id, series.Name, series.PartCategory.Id)));
        }

        [HttpPut]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("{seriesId}")]
        public async Task<ActionResult<SeriesDto>> Update(string categoryId, Guid seriesId, UpdateSeriesDto updateSeriesDto)
        {
            var category = await _partCategoriesRepository.GetAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            var series = await _seriesRepository.GetAsync(categoryId, seriesId);

            if (series == null)
            {
                return NotFound();
            }

            series.Name = updateSeriesDto.Name;

            await _seriesRepository.UpdateAsync(series);

            return Ok(new SeriesDto(series.Id, series.Name, series.PartCategory.Id));
        }

        [HttpDelete]
        [Route("{seriesid}")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult> Delete(string categoryId, Guid seriesId)
        {
            var category = await _partCategoriesRepository.GetAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            var series = await _seriesRepository.GetAsync(categoryId, seriesId);

            if (series == null)
            {
                return NotFound();
            }

            await _seriesRepository.DeleteAsync(series);

            return NoContent();
        }
    }
}
