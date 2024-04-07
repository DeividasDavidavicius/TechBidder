using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Utils;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Backend_PcAuction.Services
{
    public interface ICalculationsService
    {
        PsuCalcResultDto CalculatePSU(Part motherboard, Part cpu, Part gpu, Part ram, Part ssd, Part hdd);
        Task<List<Auction>> GeneratePcBuild(PcBuilderDataDto pcBuilderDataDto);
    }

    public class CalculationsService : ICalculationsService
    {
        private readonly IPartsRepository _partsRepository;
        private readonly IAuctionsRepository _auctionsRepository;
        private readonly IPartPricesService _partPriceService;
        public CalculationsService(IPartsRepository partsRepository, IAuctionsRepository auctionsRepository, IPartPricesService partPriceService)
        {
            _partsRepository = partsRepository;
            _auctionsRepository = auctionsRepository;
            _partPriceService = partPriceService;
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
                    else
                    {
                        psuSize += Double.Parse(part.SpecificationValue10);
                    }
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

        public async Task<List<Auction>> GeneratePcBuild(PcBuilderDataDto pcBuilderDataDto)
        {
            var motherboards = await GetParts(pcBuilderDataDto.MotherboardId, PartCategories.Motherboard);
            if (motherboards == null)
                return null;

            List<PcBuild> builds = new List<PcBuild>();

            foreach(var motherboard in motherboards)
            {
                var result = await GenerateBuildForMotherboard(motherboard, pcBuilderDataDto);
                if(result != null)
                {
                    builds.Add(result);
                }
            }

            if(builds.Count == 0)
            {
                return new List<Auction>();
            }

            // TODO: Galbut dar atkreipti demesi skaiciuojant weight score i kainu skirtuma (100/kainu skirtumas) * weight * score
            // TODO: Galbut random pasirinkti cia
            // TODO: Galbut random'e pora daliu palikti ir pora tik random parinkti 50/50 koki 
            // TODO: Pasirinkti dalis kurias jau turi? (Ten iki compatibility tik tures itaka atrodo)
            // TODO: Ieskot tik is aukcionu, kuriu current bid price <= avgPrice?

            var finalBuild = builds.OrderByDescending(b => b.TotalPrice).FirstOrDefault();
            List<Auction> auctions = new List<Auction>();

            auctions.Add(await _auctionsRepository.GetWithPartActiveCheapestAsync(finalBuild.Motherboard.Id));
            if(finalBuild.CPU != null) auctions.Add(await _auctionsRepository.GetWithPartActiveCheapestAsync(finalBuild.CPU.Id));
            if (finalBuild.GPU != null) auctions.Add(await _auctionsRepository.GetWithPartActiveCheapestAsync(finalBuild.GPU.Id));
            if (finalBuild.RAM != null) auctions.Add(await _auctionsRepository.GetWithPartActiveCheapestAsync(finalBuild.RAM.Id));
            if (finalBuild.SSD != null) auctions.Add(await _auctionsRepository.GetWithPartActiveCheapestAsync(finalBuild.SSD.Id));
            if (finalBuild.HDD != null) auctions.Add(await _auctionsRepository.GetWithPartActiveCheapestAsync(finalBuild.HDD.Id));
            if (finalBuild.PSU != null) auctions.Add(await _auctionsRepository.GetWithPartActiveCheapestAsync(finalBuild.PSU.Id));

            return auctions;
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

        public async Task<PcBuild> GenerateBuildForMotherboard(Part motherboard, PcBuilderDataDto pcBuilderDataDto)
        {
            var cpuList = pcBuilderDataDto.CpuId == null ? null : await GetParts(pcBuilderDataDto.CpuId, PartCategories.CPU);
            var gpuList = pcBuilderDataDto.GpuId == null ? null : await GetParts(pcBuilderDataDto.GpuId, PartCategories.GPU);
            var ramList = pcBuilderDataDto.RamId == null ? null : await GetParts(pcBuilderDataDto.RamId, PartCategories.RAM);
            var ssdList = pcBuilderDataDto.SsdId == null ? null : await GetParts(pcBuilderDataDto.SsdId, PartCategories.SSD);
            var hddList = pcBuilderDataDto.HddId == null ? null : await GetParts(pcBuilderDataDto.HddId, PartCategories.HDD);
            var psuList = await GetParts("ANY", PartCategories.PSU);

            if(cpuList != null)
            {
                cpuList = CheckCpuCompatibility(motherboard, cpuList);
                if (cpuList == null) return null;
            }

            if (ramList != null) 
            {
                ramList = CheckRamCompatibility(motherboard, ramList);
                if(ramList == null) return null;
            }

            if(ssdList != null)
            {
                ssdList = CheckSsdCompatibility(motherboard, ssdList);
                if (ssdList == null) return null;
            }

            if (hddList != null)
            {
                hddList = CheckSsdCompatibility(motherboard, hddList);
                if (hddList == null) return null;
            }


            motherboard.AveragePrice = await _partPriceService.GetPriceAverageAsync(motherboard.Id);


            if (cpuList != null) cpuList = await GetPrices(cpuList);
            if (gpuList != null) gpuList = await GetPrices(gpuList);
            if (ramList != null) ramList = await GetPrices(ramList);
            if (ssdList != null) ssdList = await GetPrices(ssdList);
            if (hddList != null) hddList = await GetPrices(hddList);
            if (psuList != null) psuList = await GetPrices(psuList);

            PcBuild build = new PcBuild();

            build.Motherboard = motherboard;
            build.CPU = FindCheapestPart(cpuList);
            build.GPU = FindCheapestPart(gpuList);
            build.RAM = FindCheapestPart(ramList);
            build.SSD = FindCheapestPart(ssdList);
            build.HDD = FindCheapestPart(hddList);

            var psuCalcResult = CalculatePSU(build.Motherboard, build.CPU, build.GPU, build.RAM, build.SSD, build.HDD);
            var psuSize = psuCalcResult.CalculatedWattage > psuCalcResult.RecommendedWattage ? psuCalcResult.CalculatedWattage : psuCalcResult.RecommendedWattage;

            var suitablePSUs = psuList.Where(p => Double.Parse(p.SpecificationValue1) >= psuSize);
            build.PSU = suitablePSUs.OrderBy(p => p.AveragePrice).FirstOrDefault();

            build.TotalPrice = (build.Motherboard?.AveragePrice ?? 0) +
              (build.CPU?.AveragePrice ?? 0) +
              (build.GPU?.AveragePrice ?? 0) +
              (build.RAM?.AveragePrice ?? 0) +
              (build.SSD?.AveragePrice ?? 0) +
              (build.HDD?.AveragePrice ?? 0) +
              (build.PSU?.AveragePrice ?? 0);

            if (build.TotalPrice > pcBuilderDataDto.Budget)
                return null;

            if(cpuList != null) cpuList = cpuList.Where(p => CompareParts(Double.Parse(build.CPU.SpecificationValue5), Double.Parse(build.CPU.SpecificationValue2), Double.Parse(p.SpecificationValue5), Double.Parse(p.SpecificationValue2)) != -1).ToList();
            if(gpuList != null) gpuList = gpuList.Where(p => CompareParts(Double.Parse(build.GPU.SpecificationValue3), Double.Parse(build.GPU.SpecificationValue2), Double.Parse(p.SpecificationValue3), Double.Parse(p.SpecificationValue2)) != -1).ToList();
            if(ramList != null) ramList = ramList.Where(p => CompareParts(Double.Parse(build.RAM.SpecificationValue1), Double.Parse(build.RAM.SpecificationValue2), Double.Parse(p.SpecificationValue1), Double.Parse(p.SpecificationValue2)) != -1).ToList();
            if(hddList != null) hddList = hddList.Where(p => CompareParts(Double.Parse(build.HDD.SpecificationValue1), Double.Parse(build.HDD.SpecificationValue2), Double.Parse(p.SpecificationValue1), Double.Parse(p.SpecificationValue2)) != -1).ToList();
            if(ssdList != null) ssdList = ssdList.Where(p => CompareParts(Double.Parse(build.SSD.SpecificationValue1), Double.Parse(build.SSD.SpecificationValue4), Double.Parse(p.SpecificationValue1), Double.Parse(p.SpecificationValue4)) != -1).ToList();

            if (cpuList != null) cpuList = cpuList.Where(p => CheckChangeOverBudget(pcBuilderDataDto.Budget, build.TotalPrice, build.CPU.AveragePrice, p.AveragePrice) != -1).ToList();
            if (gpuList != null) gpuList = gpuList.Where(p => CheckChangeOverBudget(pcBuilderDataDto.Budget, build.TotalPrice, build.GPU.AveragePrice, p.AveragePrice) != -1).ToList();
            if (ramList != null) ramList = ramList.Where(p => CheckChangeOverBudget(pcBuilderDataDto.Budget, build.TotalPrice, build.RAM.AveragePrice, p.AveragePrice) != -1).ToList();
            if (hddList != null) hddList = hddList.Where(p => CheckChangeOverBudget(pcBuilderDataDto.Budget, build.TotalPrice, build.HDD.AveragePrice, p.AveragePrice) != -1).ToList();
            if (ssdList != null) ssdList = ssdList.Where(p => CheckChangeOverBudget(pcBuilderDataDto.Budget, build.TotalPrice, build.SSD.AveragePrice, p.AveragePrice) != -1).ToList();

            int iterationsCount = 100;
            for(int i = 0; i < iterationsCount; i++)
            {
                PcBuild randomBuild = GenerateRandomBuild(cpuList, gpuList, ramList, ssdList, hddList, psuList, motherboard, pcBuilderDataDto.Budget);
                var score = CalculateScore(randomBuild, build);
                if(randomBuild.TotalPrice <= pcBuilderDataDto.Budget && score > 0)
                {
                    build = randomBuild;
                }
            }

            return build;
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

        public async Task<List<Part>> GetPrices(List<Part> parts)
        {
            foreach(var part in parts)
            {
                part.AveragePrice = await _partPriceService.GetPriceAverageAsync(part.Id);
            }
            return parts;
        }

        public Part FindCheapestPart(List<Part> parts)
        {
            if (parts == null)
                return null;

            return parts.OrderBy(p => p.AveragePrice).FirstOrDefault();
        }

        public double CompareParts(double oldPartSpec1, double oldPartSpec2, double newPartSpec1, double newPartSpec2)
        {
            if (oldPartSpec1 > newPartSpec1)
                return -1;
            else if (oldPartSpec1 < newPartSpec1)
                return 1;
            else if (oldPartSpec2 > newPartSpec2)
                return -1;
            else if (oldPartSpec2 < newPartSpec2)
                return 1;

            return 0;
        }

        public double CheckChangeOverBudget(double budget, double setupPrice, double oldPartPrice, double newPartPrice)
        {
            if (setupPrice + newPartPrice - oldPartPrice > budget)
                return -1;
            return 0;
        }

        public PcBuild GenerateRandomBuild(List<Part> cpuList, List<Part> gpuList, List<Part> ramList,
                                           List<Part> ssdList, List<Part> hddList, List<Part> psuList, Part motherboard, double budget)
        {
            PcBuild randomBuild = new PcBuild();

            Random random = new Random();
            randomBuild.CPU = SelectRandomPart(cpuList, random);
            randomBuild.GPU = SelectRandomPart(gpuList, random);
            randomBuild.RAM = SelectRandomPart(ramList, random);
            randomBuild.SSD = SelectRandomPart(ssdList, random);
            randomBuild.HDD = SelectRandomPart(hddList, random);
            randomBuild.Motherboard = motherboard;

            var psuCalcResult = CalculatePSU(randomBuild.Motherboard, randomBuild.CPU, randomBuild.GPU, randomBuild.RAM, randomBuild.SSD, randomBuild.HDD);
            var psuSize = psuCalcResult.CalculatedWattage > psuCalcResult.RecommendedWattage ? psuCalcResult.CalculatedWattage : psuCalcResult.RecommendedWattage;
            var suitablePSUs = psuList.Where(p => Double.Parse(p.SpecificationValue1) >= psuSize);
            randomBuild.PSU = suitablePSUs.OrderBy(p => p.AveragePrice).FirstOrDefault();

            randomBuild.TotalPrice = (randomBuild.CPU?.AveragePrice ?? 0) +
                                     (randomBuild.GPU?.AveragePrice ?? 0) +
                                     (randomBuild.RAM?.AveragePrice ?? 0) +
                                     (randomBuild.SSD?.AveragePrice ?? 0) +
                                     (randomBuild.HDD?.AveragePrice ?? 0) +
                                     (randomBuild.PSU?.AveragePrice ?? 0) +
                                     (randomBuild.Motherboard?.AveragePrice ?? 0);

            return randomBuild;
        }

        public  Part SelectRandomPart(List<Part> parts, Random random)
        {
            if (parts != null)
            {
                return parts[random.Next(parts.Count)];
            }

            return null;
        }

        public double CalculateScore(PcBuild randomBuild, PcBuild oldBuild)
        {
            double cpuWeight = 0.2;
            double gpuWeight = 0.3;
            double ramWeight = 0.15;
            double ssdWeight = 0.15;
            double hddWeight = 0.1;

            var cpuScore = 0.0;
            var gpuScore = 0.0;
            var ramScore = 0.0;
            var ssdScore = 0.0;
            var hddScore = 0.0;

            if(oldBuild.CPU != null) cpuScore = CompareParts(Double.Parse(oldBuild.CPU.SpecificationValue5), Double.Parse(oldBuild.CPU.SpecificationValue2), Double.Parse(randomBuild.CPU.SpecificationValue5), Double.Parse(randomBuild.CPU.SpecificationValue2));
            if(oldBuild.GPU != null) gpuScore = CompareParts(Double.Parse(oldBuild.GPU.SpecificationValue3), Double.Parse(oldBuild.GPU.SpecificationValue2), Double.Parse(randomBuild.GPU.SpecificationValue3), Double.Parse(randomBuild.GPU.SpecificationValue2));
            if(oldBuild.RAM != null) ramScore = CompareParts(Double.Parse(oldBuild.RAM.SpecificationValue1), Double.Parse(oldBuild.RAM.SpecificationValue2), Double.Parse(randomBuild.RAM.SpecificationValue1), Double.Parse(randomBuild.RAM.SpecificationValue2));
            if(oldBuild.SSD != null) ssdScore = CompareParts(Double.Parse(oldBuild.SSD.SpecificationValue1), Double.Parse(oldBuild.SSD.SpecificationValue4), Double.Parse(randomBuild.SSD.SpecificationValue1), Double.Parse(randomBuild.SSD.SpecificationValue4));
            if(oldBuild.HDD != null) hddScore = CompareParts(Double.Parse(oldBuild.HDD.SpecificationValue1), Double.Parse(oldBuild.HDD.SpecificationValue2), Double.Parse(randomBuild.HDD.SpecificationValue1), Double.Parse(randomBuild.HDD.SpecificationValue2));

            double score = cpuScore * cpuWeight +
                           gpuScore * gpuWeight +
                           ramScore * ramWeight +
                           ssdScore * ssdWeight +
                           hddScore * hddWeight;
            return score;
        }
    }
}
