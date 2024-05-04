using Backend_PcAuction.Data.DbSeeders;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Utils;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class PartCategorySeederTests
    {
        private readonly Mock<IPartCategoriesRepository> _partCategoriesRepositoryMock;

        private readonly PartCategorySeeder _partCategorySeeder;

        public PartCategorySeederTests()
        {
            _partCategoriesRepositoryMock = new Mock<IPartCategoriesRepository>();
            _partCategorySeeder = new PartCategorySeeder(_partCategoriesRepositoryMock.Object);
        }

        [Fact]
        public async Task SeedAsync_AllNonExistingCategories_CreatesAllCategory()
        {
            _partCategoriesRepositoryMock.Setup(m => m.GetAsync(PartCategories.CPU)).ReturnsAsync((PartCategory)null);
            _partCategoriesRepositoryMock.Setup(m => m.GetAsync(PartCategories.GPU)).ReturnsAsync((PartCategory)null);
            _partCategoriesRepositoryMock.Setup(m => m.GetAsync(PartCategories.RAM)).ReturnsAsync((PartCategory)null);
            _partCategoriesRepositoryMock.Setup(m => m.GetAsync(PartCategories.SSD)).ReturnsAsync((PartCategory)null);
            _partCategoriesRepositoryMock.Setup(m => m.GetAsync(PartCategories.HDD)).ReturnsAsync((PartCategory)null);
            _partCategoriesRepositoryMock.Setup(m => m.GetAsync(PartCategories.PSU)).ReturnsAsync((PartCategory)null);
            _partCategoriesRepositoryMock.Setup(m => m.GetAsync(PartCategories.Motherboard)).ReturnsAsync((PartCategory)null);

            await _partCategorySeeder.SeedAsync();

            _partCategoriesRepositoryMock.Verify(m => m.CreateAsync(It.IsAny<PartCategory>()), Times.Exactly(7));
        }
    }
}
