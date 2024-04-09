using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Services;
using Backend_PcAuction.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using static Backend_PcAuction.Data.Dtos.CompatiblePartsDto;

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

            if (pcBuilderDataDto.MotherboardAlreadyHave && pcBuilderDataDto.MotherboardId == "ANY")
                return UnprocessableEntity("Select specific motherboard");

            if (pcBuilderDataDto.CpuAlreadyHave && pcBuilderDataDto.CpuId == "ANY")
                return UnprocessableEntity("Select specific CPU");

            if (pcBuilderDataDto.GpuAlreadyHave && pcBuilderDataDto.GpuId == "ANY")
                return UnprocessableEntity("Select specific GPU");

            if (pcBuilderDataDto.RamAlreadyHave && pcBuilderDataDto.RamId == "ANY")
                return UnprocessableEntity("Select specific RAM");

            if (pcBuilderDataDto.SsdAlreadyHave && pcBuilderDataDto.SsdId == "ANY")
                return UnprocessableEntity("Select specific SSD");

            if (pcBuilderDataDto.HddAlreadyHave && pcBuilderDataDto.HddId == "ANY")
                return UnprocessableEntity("Select specific HDD");

            var pcBuildAuctions = new List<Auction>();
            try
            {
                pcBuildAuctions = await _calculationsService.GeneratePcBuild(pcBuilderDataDto);
            }
            catch (Exception ex)
            {
                return UnprocessableEntity("Could not generate PC build");
            }

            if (pcBuildAuctions.Count == 0)
            {
                return UnprocessableEntity("Could not generate PC build");
            }

            return Ok(pcBuildAuctions.Select(auction =>
                new AuctionWithAvgPriceDto(auction.Id, auction.Name, auction.Description, auction.CreationDate, auction.StartDate, auction.EndDate,
                    auction.MinIncrement, auction.Condition, auction.Manufacturer, auction.ImageUri, auction.Status, auction.Part.AveragePrice, auction.UserId, auction.Part.Name, auction.Part.Category.Id)));
        }

        [HttpPost]
        [Route("compatibility")]
        public async Task<ActionResult<IEnumerable<CompatiblePartsResultDto>>> GetPartCompatibility(CompatiblePartsDataDto pcBuilderDataDto)
        {
            var result = await _calculationsService.GetCompatibleParts(pcBuilderDataDto);

            return Ok(result);
        }
    }
}
