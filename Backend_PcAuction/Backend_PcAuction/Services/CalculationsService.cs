using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Utils;

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
            var motherboards = await GetMotherboards(pcBuilderDataDto.MotherboardId);
            if (motherboards == null)
                return null;

            
            foreach(var motherboard in motherboards)
            {
                CheckCompatibility(motherboard);
            }
            
            /*
            ParseData(pcBuilderDataDto.CpuId);
            ParseData(pcBuilderDataDto.GpuId);
            ParseData(pcBuilderDataDto.RamId);
            ParseData(pcBuilderDataDto.SsdId);
            ParseData(pcBuilderDataDto.HddId); */
            //ParseData(pcBuilderDataDto.PsuId); // maybe only recommend PSU
            var a = 123;
            return motherboards;
        }

        public async Task<List<Part>> GetMotherboards(string id)
        {
            if(id.Equals("ANY"))
            {
                return await _partsRepository.GetManyFromActiveAuctions(PartCategories.Motherboard);
            }
            else
            {
                Guid guid;
                if (!Guid.TryParse(id, out guid))
                {
                    return null;
                }

                var part = await _partsRepository.GetFromActiveAuctions(PartCategories.Motherboard, guid);
                return new List<Part> { part };
            }
        }

        public void CheckCompatibility(Part motherboard)
        {
            var a = 123123;
        }
    }
}
