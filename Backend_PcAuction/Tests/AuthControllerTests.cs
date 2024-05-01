using Backend_PcAuction.Controllers;
using Backend_PcAuction.Data.Dtos;
using Backend_PcAuction.Data.Entities;
using Backend_PcAuction.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stripe;
using System.Security.Claims;

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
        public async Task Register_ValidData_ShouldReturnUserObject()
        {
            var registerDto = new RegisterUserDto ("User1", "user1@gmail.com", "UserPsw1&&&");

            _userManagerMock
                .Setup(v => v.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _controller.Register(registerDto);

           var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
           var newUserDto = Assert.IsType<NewUserDto>(createdAtActionResult.Value);
           var statusCodeResult = Assert.IsType<CreatedAtActionResult>(result);

           Assert.Equal("User1", newUserDto.UserName);
           Assert.Equal(201, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task Login_ValidData_ShouldReturnExpectedData()
        {
            var user = new User { UserName = "User1", Id = "1" };
            var loginDto = new LoginDto("User1", "UserPsw1&&&");
            var expectedAccessToken = "access";
            var expectedRefreshToken = "refresh";


            _userManagerMock
                .Setup(v => v.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            _userManagerMock
                .Setup(v => v.CheckPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _userManagerMock
                .Setup(v => v.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string>());
            _jwtTokenServiceMock
                .Setup(v => v.CreateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(() => expectedAccessToken);
            _jwtTokenServiceMock
                .Setup(v => v.CreateRefreshToken(It.IsAny<string>()))
                .Returns(() => expectedRefreshToken);

            var result = await _controller.Login(new LoginDto("", ""));

            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            var successfulLoginDto = Assert.IsType<SuccessfulLoginDto>(okObjectResult.Value);
            var statusCodeResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(expectedAccessToken, successfulLoginDto.AccessToken);
            Assert.Equal(expectedRefreshToken, successfulLoginDto.RefreshToken);
            Assert.Equal(200, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task Logout_ValidToken_ReturnsOk()
        {
            var refreshAccessTokenDto = new RefreshAccessTokenDto("validRefreshToken");


            var userId = "1";
            var user = new User { UserName = "User1", Id = userId };

            _userManagerMock
                .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            var identity = new ClaimsIdentity(claims);
            var userPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = userPrincipal
                }
            };

            var result = await _controller.Logout(refreshAccessTokenDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Succesfully logged out!", okResult.Value);
        }
    }
}