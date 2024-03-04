using Backend_PcAuction.Auth;
using Backend_PcAuction.Auth.Model;
using Backend_PcAuction.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Backend_PcAuction.Auth.Models.AuthDto;

namespace Backend_PcAuction.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/v1")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(UserManager<User> userManager, IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterUserDto registerUserDto)
        {
            var user = await _userManager.FindByNameAsync(registerUserDto.UserName);

            if (user != null)
                return BadRequest("User with this username already exists!");

            var newUser = new User
            {
                UserName = registerUserDto.UserName,
                Email = registerUserDto.Email,
            };

            var createUserResult = await _userManager.CreateAsync(newUser, registerUserDto.Password);
            if (!createUserResult.Succeeded)
                return BadRequest("Could not create an user.");

            await _userManager.AddToRoleAsync(newUser, UserRoles.RegisteredUser);

            return CreatedAtAction(nameof(Register), new UserDto(newUser.Id, newUser.UserName, newUser.Email));
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user == null)
                return BadRequest("Username or password is invalid!");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!isPasswordValid)
                return BadRequest("Username or password is invalid!");

            //user.Relogin = false;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = _jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);
            //var refreshToken = _jwtTokenService.CreateRefreshToken(user.Id);

            return Ok(new SuccessfulLoginDto(accessToken, "refreshToken"));
        }
    }
}
