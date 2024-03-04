using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend_PcAuction.Auth
{
    public interface IJwtTokenService
    {
        string CreateAccessToken(string userName, string userId, IEnumerable<string> UserRoles);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly string _issuer;
        private readonly string _audience;
        private SymmetricSecurityKey _authSignKey;

        public JwtTokenService(IConfiguration configuration)
        {
            _issuer = configuration["JWT:ValidIssuer"];
            _audience = configuration["JWT:ValidAudience"];
            _authSignKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));
        }

        public string CreateAccessToken(string userName, string userId, IEnumerable<string> UserRoles)
        {
            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Name, userName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, userId)
            };

            authClaims.AddRange(UserRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

            var accessSecurityToken = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                expires: DateTime.UtcNow.AddMinutes(1), // TODO change to 5mins
                claims: authClaims,
                signingCredentials: new SigningCredentials(_authSignKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(accessSecurityToken);
        }

    }
}
