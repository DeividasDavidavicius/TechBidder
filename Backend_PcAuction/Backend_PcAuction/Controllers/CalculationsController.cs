using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Services;
using Backend_PcAuction.Utils;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [Route("api/v1/")]
    public class CalculationsController : ControllerBase
    {
        private readonly IPartsRepository _partsRepository;
        private readonly ICalculationsService _calculationsService;

        public CalculationsController(IPartsRepository partsRepository, ICalculationsService calculationsService)
        {
            _partsRepository = partsRepository;
            _calculationsService = calculationsService;
        }

        [HttpPost]
        [Route("psucalculator")]
        public async Task<ActionResult<PsuCalcResultDto>> GetRecommendedPsu(PsuCalcDataDto psuCalcDataDto)
        {
            Part motherboard = await _partsRepository.GetAsync(PartCategories.Motherboard, psuCalcDataDto.MotherboardId);
            Part cpu = await _partsRepository.GetAsync(PartCategories.CPU, psuCalcDataDto.CpuId);
            Part gpu = await _partsRepository.GetAsync(PartCategories.GPU, psuCalcDataDto.GpuId);
            Part ram = await _partsRepository.GetAsync(PartCategories.RAM, psuCalcDataDto.RamId);
            Part hdd = await _partsRepository.GetAsync(PartCategories.HDD, psuCalcDataDto.HddId);
            Part ssd = await _partsRepository.GetAsync(PartCategories.SSD, psuCalcDataDto.SsdId);

            if (psuCalcDataDto.MotherboardId != null && motherboard == null)
                return NotFound();

            if (psuCalcDataDto.CpuId != null && cpu == null)
                return NotFound();

            if (psuCalcDataDto.GpuId != null && gpu == null)
                return NotFound();

            if (psuCalcDataDto.RamId != null && ram == null)
                return NotFound();

            if (psuCalcDataDto.SsdId != null && ssd == null)
                return NotFound();

            if (psuCalcDataDto.HddId != null && hdd == null)
                return NotFound();

            var result = _calculationsService.CalculatePSU(motherboard, cpu, gpu, ram, ssd, hdd);
            return result;
        }

        [HttpPost]
        [Route("pcbuildgenerator")]
        public async Task<ActionResult<IEnumerable<AuctionDto>>> GetPcBuild(PcBuilderDataDto pcBuilderDataDto)
        {
            if (pcBuilderDataDto.MotherboardId == null)
                return NotFound();

            var pcBuildAuctions = await _calculationsService.GeneratePcBuild(pcBuilderDataDto);

            return Ok(pcBuildAuctions.Select(auction =>
                new AuctionWithPartNameDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate, auction.EndDate,
                    auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.ImageUri, auction.Status, auction.UserId, auction.Part.Name, auction.Part.Category.Id)));
        }
    }
}
