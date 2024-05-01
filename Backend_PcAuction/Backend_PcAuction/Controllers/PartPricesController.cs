using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Services;
using Backend_PcAuction.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/parts/{partId}/prices")]
    public class PartPricesController : ControllerBase
    {
        private readonly IPartPricesService _partPriceService;
        private readonly IPartsPricesRepository _partPricesRepository;
        private readonly IPartsRepository _partsRepository;

        public PartPricesController(IPartPricesService partPriceService, IPartsRepository partsRepository, IPartsPricesRepository partPricesRepository)
        {
            _partPriceService = partPriceService;
            _partsRepository = partsRepository;
            _partPricesRepository = partPricesRepository;
        }


        // Temporary
        [HttpPost]
        public async Task<ActionResult> Create(Guid partId, CreatePartPriceDto price)
        {
            var part = await _partsRepository.GetForAnyCategoryAsync(partId);

            if (part == null)
            {
                return NotFound();
            }


            var partPrice = new PartPrice
            {
                Type = PartPriceTypes.EbayAverage,
                Price = price.Price, 
                PriceCheckDate = DateTime.UtcNow,
                Part = part
            };

            await _partPricesRepository.CreateAsync(partPrice);

            return NoContent();
        }

        [HttpGet]
        [Route("averagePrice")]
        public async Task<ActionResult<PartPriceDto>> Get(Guid partId)
        {
            var part = await _partsRepository.GetForAnyCategoryAsync(partId);

            if (part == null)
            {
                return NotFound();
            }

            var averagePrice = await _partPriceService.GetPriceAverageAsync(partId);

            return Ok(new PartPriceDto(part.Id, averagePrice));
        }
    }
}
