using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Utils;

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
            await CreateMotherboard();
        }

        private async Task CreateCPU()
        {
            var existingCategory = await _partCategoriesRepository.GetAsync(PartCategories.CPU);
            if (existingCategory == null)
            {
                var categoryCPU = new PartCategory
                {
                    Id = PartCategories.CPU,
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
            var existingCategory = await _partCategoriesRepository.GetAsync(PartCategories.GPU);
            if (existingCategory == null)
            {
                var categoryCPU = new PartCategory
                {
                    Id = PartCategories.GPU,
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
            var existingCategory = await _partCategoriesRepository.GetAsync(PartCategories.RAM);
            if (existingCategory == null)
            {
                var categoryCPU = new PartCategory
                {
                    Id = PartCategories.RAM,
                    SpecificationName1 = "Memory size (GB)",
                    SpecificationName2 = "Memory speed (MHz)",
                    SpecificationName3 = "DDR version",
                    SpecificationName4 = "Module count",
                    SpecificationName10 = "Wattage (W)"
                };

                await _partCategoriesRepository.CreateAsync(categoryCPU);
            }
        }

        private async Task CreatePSU()
        {
            var existingCategory = await _partCategoriesRepository.GetAsync(PartCategories.PSU);
            if (existingCategory == null)
            {
                var categoryCPU = new PartCategory
                {
                    Id = PartCategories.PSU,
                    SpecificationName1 = "Wattage (W)"
                };

                await _partCategoriesRepository.CreateAsync(categoryCPU);
            }
        }

        private async Task CreateHDD()
        {
            var existingCategory = await _partCategoriesRepository.GetAsync(PartCategories.HDD);
            if (existingCategory == null)
            {
                var categoryCPU = new PartCategory
                {
                    Id = PartCategories.HDD,
                    SpecificationName1 = "Capacity (GB)",
                    SpecificationName2 = "RPM",
                    SpecificationName10 = "Wattage (W)"
                };

                await _partCategoriesRepository.CreateAsync(categoryCPU);
            }
        }

        private async Task CreateSSD()
        {
            var existingCategory = await _partCategoriesRepository.GetAsync(PartCategories.SSD);
            if (existingCategory == null)
            {
                var categoryCPU = new PartCategory
                {
                    Id = PartCategories.SSD,
                    SpecificationName1 = "Capacity (GB)",
                    SpecificationName2 = "NVMe generation",
                    SpecificationName3 = "SATA interface",
                    SpecificationName4 = "Reading speed (MB/s)",
                    SpecificationName5 = "Writing speed (MB/s)",
                    SpecificationName10 = "Wattage (W)"
                };

                await _partCategoriesRepository.CreateAsync(categoryCPU);
            }
        }

        private async Task CreateMotherboard()
        {
            var existingCategory = await _partCategoriesRepository.GetAsync(PartCategories.Motherboard);
            if (existingCategory == null)
            {
                var categoryCPU = new PartCategory
                {
                    Id = PartCategories.Motherboard,
                    SpecificationName1 = "CPU socket type",
                    SpecificationName2 = "Memory DDR version",
                    SpecificationName3 = "Memory slots",
                    SpecificationName4 = "Maximum memory capacity (GB)",
                    SpecificationName5 = "NVMe generation (1)",
                    SpecificationName6 = "NVMe slots (1)",
                    SpecificationName7 = "NVMe generation (2)",
                    SpecificationName8 = "NVMe slots (2)",
                    SpecificationName9 = "Sata slots",
                    SpecificationName10 = "Wattage (W)"
                };

                await _partCategoriesRepository.CreateAsync(categoryCPU);
            }
        }
    }
}
