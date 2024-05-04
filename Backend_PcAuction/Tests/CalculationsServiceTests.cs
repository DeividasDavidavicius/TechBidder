using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Services;
using Backend_PcAuction.Utils;
using Moq;
using Stripe.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Backend_PcAuction.Data.Dtos.CompatiblePartsDto;

namespace Tests
{
    public class CalculactionsServiceTests
    {
        private readonly Mock<IPartsRepository> _partsRepositoryMock;
        private readonly Mock<IAuctionsRepository> _auctionsRepositoryMock;
        private readonly Mock<IPartPricesService> _partPriceServiceMock;
        private readonly CalculationsService _calculationsService;

        public CalculactionsServiceTests()
        {
            _partsRepositoryMock = new Mock<IPartsRepository>();
            _auctionsRepositoryMock = new Mock<IAuctionsRepository>();
            _partPriceServiceMock = new Mock<IPartPricesService>();
            _calculationsService = new CalculationsService(_partsRepositoryMock.Object, _auctionsRepositoryMock.Object, _partPriceServiceMock.Object);
        }

        [Fact]
        public void CalculatePSU_AllComponentsPresent_ReturnsCorrectResult()
        {
            var motherboard = new Part { Category = new PartCategory { Id = PartCategories.Motherboard }, SpecificationValue10 = "100" };
            var cpu = new Part { Category = new PartCategory { Id = PartCategories.CPU }, SpecificationValue10 = "50" };
            var gpu = new Part { Category = new PartCategory { Id = PartCategories.GPU }, SpecificationValue9 = "200", SpecificationValue10 = "200" };
            var ram = new Part { Category = new PartCategory { Id = PartCategories.RAM }, SpecificationValue10 = "10", SpecificationValue4 = "2" };
            var ssd = new Part { Category = new PartCategory { Id = PartCategories.SSD }, SpecificationValue10 = "20" };
            var hdd = new Part { Category = new PartCategory { Id = PartCategories.HDD }, SpecificationValue10 = "30" };

            var result = _calculationsService.CalculatePSU(motherboard, cpu, gpu, ram, ssd, hdd);

            Assert.Equal(500, result.CalculatedWattage);
            Assert.Equal(200, result.RecommendedWattage);
        }

        [Fact]
        public void CalculatePSU_SomeComponentsNull_ReturnsCorrectResult()
        {
            var motherboard = new Part { Category = new PartCategory { Id = PartCategories.Motherboard }, SpecificationValue10 = "100" };
            var cpu = new Part { Category = new PartCategory { Id = PartCategories.CPU }, SpecificationValue10 = "50" };
            var gpu = new Part { Category = new PartCategory { Id = PartCategories.GPU }, SpecificationValue9 = "200", SpecificationValue10 = "200" };
            Part ram = null;
            var ssd = new Part { Category = new PartCategory { Id = PartCategories.SSD }, SpecificationValue10 = "20" };
            var hdd = new Part { Category = new PartCategory { Id = PartCategories.HDD }, SpecificationValue10 = "30" };

            var result = _calculationsService.CalculatePSU(motherboard, cpu, gpu, ram, ssd, hdd);

            Assert.Equal(450, result.CalculatedWattage);
            Assert.Equal(200, result.RecommendedWattage);
        }

        [Fact]
        public void CalculatePSU_AllComponentsNull_ReturnsCorrectResult()
        {
            Part motherboard = null;
            Part cpu = null;
            Part gpu = null;
            Part ram = null;
            Part ssd = null;
            Part hdd = null;

            var result = _calculationsService.CalculatePSU(motherboard, cpu, gpu, ram, ssd, hdd);

            Assert.Equal(50, result.CalculatedWattage);
            Assert.Equal(-1, result.RecommendedWattage);
        }

        [Fact]
        public void GetCompatibleParts_ValidData_ReturnsCompatibleParts()
        {
            var compatiblePartsDataDto = new CompatiblePartsDataDto(PartCategories.Motherboard, Guid.NewGuid(), PartCategories.CPU);

            var partsForCategory = new List<Part> { new Part { SpecificationValue1 = "Spec1", Id = Guid.NewGuid(), Category = new PartCategory { Id = PartCategories.Motherboard } } };
            _partsRepositoryMock.Setup(repo => repo.GetManyAsync(compatiblePartsDataDto.CompatibleCategoryId))
                                .ReturnsAsync(partsForCategory);

            if (compatiblePartsDataDto.CategoryId == PartCategories.Motherboard)
            {
                _partsRepositoryMock.Setup(repo => repo.GetAsync(PartCategories.Motherboard, compatiblePartsDataDto.PartId))
                                    .ReturnsAsync(new Part { SpecificationValue1 = "Spec1", Id = Guid.NewGuid(), Category = new PartCategory { Id = PartCategories.CPU } });
            }

            var result = _calculationsService.GetCompatibleParts(compatiblePartsDataDto);

            Assert.NotNull(result);
        }

        [Fact]
        public void GeneratePcBuild_ValidData_ReturnsAuctionsToBidOn()
        {
            var motherboardId = Guid.NewGuid();
            var cpuId = Guid.NewGuid();
            var ramId = Guid.NewGuid();

            PcBuilderDataDto pcBuilderDataDto = new PcBuilderDataDto(motherboardId.ToString(), cpuId.ToString(), null, ramId.ToString(), 
                null, null, null, false, false, false, true, false, false, true, 1000, 0); ;

            _partsRepositoryMock.Setup(repo => repo.GetFromActiveAuctions(PartCategories.Motherboard, motherboardId))
                .ReturnsAsync(new Part { SpecificationValue1 = "SocketType1", SpecificationValue2 = "DDRVersion", SpecificationValue3 = "1", 
                    SpecificationValue4 = "16", SpecificationValue10 = "50", Category = new PartCategory { Id = PartCategories.Motherboard } });

            _partsRepositoryMock.Setup(repo => repo.GetFromActiveAuctions(PartCategories.CPU, cpuId))
                .ReturnsAsync(new Part { SpecificationValue1 = "SocketType1", SpecificationValue2 = "2", SpecificationValue5 = "2",
                    SpecificationValue10 = "70", Category = new PartCategory { Id = PartCategories.CPU } });

            _partsRepositoryMock.Setup(repo => repo.GetAsync(PartCategories.RAM, ramId))
                .ReturnsAsync(new Part { SpecificationValue3 = "DDRVersion", SpecificationValue1 = "16", SpecificationValue4 = "1", 
                    SpecificationValue10 = "5", Category = new PartCategory { Id = PartCategories.RAM } });

            _partsRepositoryMock.Setup(repo => repo.GetManyFromActiveAuctions(PartCategories.PSU))
                .ReturnsAsync(new List<Part>() { new Part { SpecificationValue1 = "1000", Category = new PartCategory { Id = PartCategories.PSU } } });
            
            var result = _calculationsService.GeneratePcBuild(pcBuilderDataDto);

            Assert.NotNull(result);
            Assert.Equal(result.Result.Count, 3); 
        }
    }
}
