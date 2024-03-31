using Backend_PcAuction.Auth.Model;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Backend_PcAuction.Data.DbSeeders
{
    public class PartCategorySeeder
    {
        private readonly IPartCategoriesRepository _partCategoriesRepository;

        public PartCategorySeeder(IPartCategoriesRepository partCategoriesRepository)
        {
            this._partCategoriesRepository = partCategoriesRepository;
        }

        public async Task SeedAsync()
        {
            await CreateCPU();
            await CreateGPU();
            await CreateRAM();
            await CreatePSU();
            await CreateHDD();
            await CreateSSD();
        }

        private async Task CreateCPU()
        {
            var existingCategory = await _partCategoriesRepository.GetAsync("CPU");
            if (existingCategory == null)
            {
                var categoryCPU = new PartCategory
                {
                    Id = "CPU",
                    SpecificationName1 = "Socket type",
                    SpecificationName2 = "Number of cores",
                    SpecificationName3 = "Number of threads",
                    SpecificationName4 = "Base clock",
                    SpecificationName5 = "Max clock",
                    SpecificationName6 = "Base clock (2)",
                    SpecificationName7 = "Max clock (2)",
                    SpecificationName10 = "Wattage (W)"
                };

                await _partCategoriesRepository.CreateAsync(categoryCPU);
            }
        }

        private async Task CreateGPU()
        {
            var existingCategory = await _partCategoriesRepository.GetAsync("GPU");
            if (existingCategory == null)
            {
                var categoryCPU = new PartCategory
                {
                    Id = "GPU",
                    SpecificationName1 = "Memory size (GB)", // probably 2nd important
                    SpecificationName2 = "Core count", // probably most important
                    SpecificationName3 = "Transistor count (million)", // can skip in calculations
                    SpecificationName4 = "Base clock (MHz)", // can skip in calculations
                    SpecificationName9 = "Suggested PSU (W)",
                    SpecificationName10 = "Wattage (W)"
                };

                await _partCategoriesRepository.CreateAsync(categoryCPU);
            }
        }

        private async Task CreateRAM()
        {
            var existingCategory = await _partCategoriesRepository.GetAsync("RAM");
            if (existingCategory == null)
            {
                var categoryCPU = new PartCategory
                {
                    Id = "RAM",
                    SpecificationName1 = "Memory size (GB)",
                    SpecificationName2 = "Memory speed (MHz)",
                    SpecificationName3 = "DDR version",
                    SpecificationName10 = "Wattage (W)"
                };

                await _partCategoriesRepository.CreateAsync(categoryCPU);
            }
        }

        private async Task CreatePSU()
        {
            var existingCategory = await _partCategoriesRepository.GetAsync("PSU");
            if (existingCategory == null)
            {
                var categoryCPU = new PartCategory
                {
                    Id = "PSU",
                    SpecificationName1 = "Wattage (W)"
                };

                await _partCategoriesRepository.CreateAsync(categoryCPU);
            }
        }

        private async Task CreateHDD()
        {
            var existingCategory = await _partCategoriesRepository.GetAsync("HDD");
            if (existingCategory == null)
            {
                var categoryCPU = new PartCategory
                {
                    Id = "HDD",
                    SpecificationName1 = "Capacity (GB)",
                    SpecificationName2 = "RPM",
                    SpecificationName10 = "Wattage (W)"
                };

                await _partCategoriesRepository.CreateAsync(categoryCPU);
            }
        }

        private async Task CreateSSD()
        {
            var existingCategory = await _partCategoriesRepository.GetAsync("SSD");
            if (existingCategory == null)
            {
                var categoryCPU = new PartCategory
                {
                    Id = "SSD",
                    SpecificationName1 = "Capacity (GB)",
                    SpecificationName2 = "NVMe Generation",
                    SpecificationName3 = "SATA interface",
                    SpecificationName4 = "Reading speed",
                    SpecificationName5 = "Writing speed",
                    SpecificationName10 = "Wattage (W)"
                };

                await _partCategoriesRepository.CreateAsync(categoryCPU);
            }
        }
    }
}
