using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Services;
using Backend_PcAuction.Utils;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<PsuCalcResultDto>> Get(PsuCalcDataDto psuCalcDataDto)
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
    }
}
