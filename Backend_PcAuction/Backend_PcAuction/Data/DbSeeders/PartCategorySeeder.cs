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
                    SpecificationName7 = "Max clock (2)"
                };

                await _partCategoriesRepository.CreateAsync(categoryCPU);
            }
        }
    }
}
