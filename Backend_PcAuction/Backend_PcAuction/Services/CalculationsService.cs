using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Utils;
using System.Runtime.CompilerServices;

namespace Backend_PcAuction.Services
{
    public interface ICalculationsService
    {
        PsuCalcResultDto CalculatePSU(Part motherboard, Part cpu, Part gpu, Part ram, Part ssd, Part hdd);
        Task<List<Part>> GeneratePcBuild(PcBuilderDataDto pcBuilderDataDto);
    }

    public class CalculationsService : ICalculationsService
    {
        public readonly IPartsRepository _partsRepository;
        public readonly IAuctionsRepository _auctionsRepository;
        public CalculationsService(IPartsRepository partsRepository, IAuctionsRepository auctionsRepository)
        {
            _partsRepository = partsRepository;
            _auctionsRepository = auctionsRepository;
        }

        public PsuCalcResultDto CalculatePSU(Part motherboard, Part cpu, Part gpu, Part ram, Part ssd, Part hdd)
        {
            List<Part> parts = new List<Part>();
            double psuSize = 0;

            parts.Add(motherboard);
            parts.Add(cpu);
            parts.Add(gpu);
            parts.Add(ram);
            parts.Add(ssd);
            parts.Add(hdd);

            foreach(Part part in parts)
            {
                if(part != null)
                {
                    if(part.Category.Id == PartCategories.RAM)
                    {
                        psuSize += Double.Parse(part.SpecificationValue10) * Int32.Parse(part.SpecificationValue4);
                    }
                    psuSize += Double.Parse(part.SpecificationValue10);
                }
            }

            double calculatedWattage = Math.Ceiling(psuSize / 50) * 50 + 50;
            double recommendedWattage = -1;

            if (gpu != null)
            {
                recommendedWattage = Double.Parse(gpu.SpecificationValue9);
            }

            return new PsuCalcResultDto(calculatedWattage, recommendedWattage);
        }

        public async Task<List<Part>> GeneratePcBuild(PcBuilderDataDto pcBuilderDataDto)
        {
            var motherboards = await GetParts(pcBuilderDataDto.MotherboardId, PartCategories.Motherboard);
            if (motherboards == null)
                return null;

            var compatibleMotherboards = new List<Part>(motherboards);

            foreach(var motherboard in motherboards)
            {
                var result = await GenerateBuildForMotherboard(motherboard, pcBuilderDataDto);
                if(result == -1)
                {
                    compatibleMotherboards.Remove(motherboard);
                }
            }
            
            // only recommend PSU
            var a = 123;
            return motherboards;
        }

        public async Task<List<Part>> GetParts(string id, string category)
        {
            if (id.Equals("ANY"))
            {
                return await _partsRepository.GetManyFromActiveAuctions(category);
            }
            else
            {
                Guid guid;
                if (!Guid.TryParse(id, out guid))
                {
                    return null;
                }

                var part = await _partsRepository.GetFromActiveAuctions(category, guid);
                return new List<Part> { part };
            }
        }

        public async Task<int> GenerateBuildForMotherboard(Part motherboard, PcBuilderDataDto pcBuilderDataDto)
        {
            var cpuList = pcBuilderDataDto.CpuId == null ? null : await GetParts(pcBuilderDataDto.CpuId, PartCategories.CPU);
            var gpuList = pcBuilderDataDto.GpuId == null ? null : await GetParts(pcBuilderDataDto.GpuId, PartCategories.GPU);
            var ramList = pcBuilderDataDto.RamId == null ? null : await GetParts(pcBuilderDataDto.RamId, PartCategories.RAM);
            var ssdList = pcBuilderDataDto.SsdId == null ? null : await GetParts(pcBuilderDataDto.SsdId, PartCategories.SSD);
            var hddList = pcBuilderDataDto.HddId == null ? null : await GetParts(pcBuilderDataDto.HddId, PartCategories.HDD);

            if(cpuList != null)
            {
                cpuList = CheckCpuCompatibility(motherboard, cpuList);
                if (cpuList == null) return -1;
            }

            if (ramList != null) 
            {
                ramList = CheckRamCompatibility(motherboard, ramList);
                if(ramList == null) return -1;
            }

            if(ssdList != null)
            {
                ssdList = CheckSsdCompatibility(motherboard, ssdList);
                if (ssdList == null) return -1;
            }

            if (hddList != null)
            {
                hddList = CheckSsdCompatibility(motherboard, hddList);
                if (hddList == null) return -1;
            }

            return 0;
        }

        public List<Part> CheckCpuCompatibility(Part motherboard, List<Part> cpuList)
        {
            cpuList = cpuList.Where(cpu => cpu.SpecificationValue1.Equals(motherboard.SpecificationValue1)).ToList();

            if (cpuList.Count == 0)
            {
                return null;
            }

            return cpuList;
        }

        public List<Part> CheckRamCompatibility(Part motherboard, List<Part> ramList)
        {
            ramList = ramList.Where(ram =>
                ram.SpecificationValue3.Equals(motherboard.SpecificationValue2) &&
                Double.Parse(ram.SpecificationValue1) <= Double.Parse(motherboard.SpecificationValue4) &&
                Double.Parse(ram.SpecificationValue4) <= Double.Parse(motherboard.SpecificationValue3)
            ).ToList();

            if (ramList.Count == 0)
            {
                return null;
            }

            return ramList;
        }

        public List<Part> CheckSsdCompatibility(Part motherboard, List<Part> ssdList)
        {
            ssdList = ssdList.Where(ssd =>
                ((ssd.SpecificationValue2 != "" && (motherboard.SpecificationValue5 != "" || motherboard.SpecificationValue7 != "")) || (ssd.SpecificationValue3 != "" && motherboard.SpecificationValue9 != ""))
            ).ToList();

            if (ssdList.Count == 0)
            {
                return null;
            }

            return ssdList;
        }

        public List<Part> CheckHddCompatibility(Part motherboard, List<Part> hddList)
        {
            hddList = hddList.Where(hdd => Double.Parse(motherboard.SpecificationValue9) > 0).ToList();

            if (hddList.Count == 0)
            {
                return null;
            }

            return hddList;
        }
    }
}
