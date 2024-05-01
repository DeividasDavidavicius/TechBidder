using System.ComponentModel.DataAnnotations;

namespace Backend_PcAuction.Data.Dtos
{

    public record RegisterUserDto([Required] string UserName, [EmailAddress][Required] string Email, [Required] string Password);
    public record LoginDto(string UserName, string Password);
    public record NewUserDto(string Id, string UserName, string Email);
    public record UserDetailsDto();
    public record SuccessfulLoginDto(string AccessToken, string RefreshToken);
    public record RefreshAccessTokenDto(string RefreshToken);
}
