using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/categories")]
    public class PartCategoriesController : ControllerBase
    {
        private readonly IPartCategoriesRepository _partCategoriesRepository;

        public PartCategoriesController(IPartCategoriesRepository partCategoriesRepository)
        {
            _partCategoriesRepository = partCategoriesRepository;
        }

        [HttpGet]
        [Route("{categoryId}")]
        public async Task<ActionResult<PartCategoryDto>> Get(string categoryId)
        {
            var category = await _partCategoriesRepository.GetAsync(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(new PartCategoryDto(category.Id, category.SpecificationName1, category.SpecificationName2,
                category.SpecificationName3,category.SpecificationName4, category.SpecificationName5, category.SpecificationName6,
                category.SpecificationName7, category.SpecificationName8, category.SpecificationName9, category.SpecificationName10));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PartCategoryDto>>> GetMany()
        {
            var categories = await _partCategoriesRepository.GetManyAsync();
            return Ok(categories.Select(category => new PartCategoryDto(category.Id, category.SpecificationName1, category.SpecificationName2,
                category.SpecificationName3, category.SpecificationName4, category.SpecificationName5, category.SpecificationName6,
                category.SpecificationName7, category.SpecificationName8, category.SpecificationName9, category.SpecificationName10)));

        }
    }
}
