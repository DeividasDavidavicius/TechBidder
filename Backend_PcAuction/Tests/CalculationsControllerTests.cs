using Backend_PcAuction.Controllers;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Data.Repositories;
using Backend_PcAuction.Services;
using Backend_PcAuction.Utils;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Backend_PcAuction.Data.Dtos.CompatiblePartsDto;

namespace Tests
{
    public class CalculationsControllerTests
    {
        private readonly Mock<IPartsRepository> _partsRepositoryMock;
        private readonly Mock<ICalculationsService> _calculationsServiceMock;
        private readonly CalculationsController _controller;

        public CalculationsControllerTests()
        {
            _partsRepositoryMock = new Mock<IPartsRepository>();
            _calculationsServiceMock = new Mock<ICalculationsService>();
            _controller = new CalculationsController(_partsRepositoryMock.Object, _calculationsServiceMock.Object);
        }

        [Fact]
        public async Task GetRecommendedPsu_ReturnsNotFound_WhenPartsDoNotExist()
        {
            var psuCalcDataDto = new PsuCalcDataDto(Guid.NewGuid(), null, null, null, null, null);

            _partsRepositoryMock.Setup(repo => repo.GetAsync(PartCategories.Motherboard, psuCalcDataDto.MotherboardId)).ReturnsAsync((Part)null);

            var result = await _controller.GetRecommendedPsu(psuCalcDataDto);

            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetRecommendedPsu_ReturnsResult_WhenPartsExist()
        {
            var psuCalcDataDto = new PsuCalcDataDto(Guid.NewGuid(), null, null, null, null, null);

            var motherboard = new Part();
            _partsRepositoryMock.Setup(repo => repo.GetAsync(PartCategories.Motherboard, psuCalcDataDto.MotherboardId)).ReturnsAsync(motherboard);

            var expectedResult = new PsuCalcResultDto(10, 10);
            _calculationsServiceMock.Setup(service => service.CalculatePSU(It.IsAny<Part>(), It.IsAny<Part>(), It.IsAny<Part>(), It.IsAny<Part>(), It.IsAny<Part>(), It.IsAny<Part>(), 0)).Returns(expectedResult);

            var result = await _controller.GetRecommendedPsu(psuCalcDataDto);

            var okResult = Assert.IsType<ActionResult<PsuCalcResultDto>>(result);
            Assert.Equal(expectedResult, okResult.Value);
        }

        [Fact]
        public async Task GetPartCompatibility_ReturnsOkResult_WhenPartListFound()
        {
            var pcBuilderDataDto = new CompatiblePartsDataDto("Cat1", Guid.NewGuid(), "Cat2");
            var expectedResult = new List<CompatiblePartsResultDto>();

            _calculationsServiceMock.Setup(service => service.GetCompatibleParts(pcBuilderDataDto)).ReturnsAsync(expectedResult);

            var result = await _controller.GetPartCompatibility(pcBuilderDataDto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var resultValue = Assert.IsAssignableFrom<IEnumerable<CompatiblePartsResultDto>>(okResult.Value);
            Assert.Equal(expectedResult, resultValue);
        }

        [Fact]
        public async Task GetPcBuild_ReturnsNotFound_WhenMotherboardIdIsNull()
        {
            var pcBuilderDataDto = new PcBuilderDataDto(null, null, null, null, null, null, null, true, true, true, true, true, true, true, 0, 0);

            var result = await _controller.GetPcBuild(pcBuilderDataDto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetPcBuild_ReturnsUnprocessableEntity_WhenMotherboardAlreadyHaveAndMotherboardIdIsANY()
        {
            var pcBuilderDataDto = new PcBuilderDataDto("ANY", null, null, null, null, null, null, true, true, true, true, true, true, true, 0, 0);

            var result = await _controller.GetPcBuild(pcBuilderDataDto);

            Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetPcBuild_ReturnsOkWithAuctionWithAvgPriceDto_WhenAllConditionsAreMet()
        {
            var pcBuilderDataDto = new PcBuilderDataDto("ValidId", null, null, null, null, null, null, true, true, true, true, true, true, true, 0, 0);

            var pcBuildAuctions = new List<AuctionWithAvgPriceDto>
            {
                new AuctionWithAvgPriceDto(Guid.NewGuid(), "Auction 1", "", DateTime.Now, DateTime.Now, DateTime.Now, 0, "", "", "", "", 0, "", "", ""),
            };

            var auctionId = Guid.NewGuid();
            var partId = Guid.NewGuid();
            var categoryId = "CPU";

            _calculationsServiceMock.Setup(service => service.GeneratePcBuild(pcBuilderDataDto)).ReturnsAsync(new List<Auction>() { new Auction() { Id = auctionId, Status = "Active", Part = new Part { Id = partId, Category = new PartCategory { Id = categoryId } } } });

            var result = await _controller.GetPcBuild(pcBuilderDataDto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var auctionDtos = Assert.IsAssignableFrom<IEnumerable<AuctionWithAvgPriceDto>>(okResult.Value);
            Assert.Equal(pcBuildAuctions.Count, auctionDtos.Count());
        }


    }
}
