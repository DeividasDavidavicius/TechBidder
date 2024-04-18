using Microsoft.AspNetCore.Identity;

namespace Backend_PcAuction.Auth.Model
{
    using Microsoft.AspNetCore.Identity;
    using System.Collections.Generic;

    public class User : IdentityUser
    {
        public string BlacklistedRefreshTokens { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string BankDetails { get; set; }

        public bool IsRefreshTokenBlacklisted(string token)
        {
            return BlacklistedRefreshTokens?.Split(',').Contains(token) ?? false;
        }

        public void AddRefreshTokenToBlacklist(string token)
        {
            if (BlacklistedRefreshTokens == null)
                BlacklistedRefreshTokens = token;
            else
                BlacklistedRefreshTokens += "," + token;
        }
    }
}
