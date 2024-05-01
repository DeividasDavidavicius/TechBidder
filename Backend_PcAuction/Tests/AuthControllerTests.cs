using Backend_PcAuction.Controllers;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
        private readonly Mock<UserManager<User>> _userManagerMock;

        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var mockUserStore = new Mock<IUserStore<User>>();
            var mockPasswordHasher = new Mock<IPasswordHasher<User>>();

            _userManagerMock = new Mock<UserManager<User>>(
                mockUserStore.Object, null, mockPasswordHasher.Object, null, null, null, null, null, null);

            _jwtTokenServiceMock = new Mock<IJwtTokenService>();

            _controller = new AuthController(_userManagerMock.Object, _jwtTokenServiceMock.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnUserObject()
        {
            var registerDto = new RegisterUserDto ("User1", "user1@gmail.com", "UserPsw1&&&");

            _userManagerMock.Setup(v => v.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Register(registerDto);

           var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
           var newUserDto = Assert.IsType<NewUserDto>(createdAtActionResult.Value);
           var statusCodeResult = Assert.IsType<CreatedAtActionResult>(result);
           Assert.Equal("User1", newUserDto.UserName);
           Assert.Equal(201, statusCodeResult.StatusCode);
        }
    }
}