using Backend_PcAuction.Auth.Model;
using Backend_PcAuction.Auth.Models;
using Microsoft.AspNetCore.Identity;

namespace Backend_PcAuction.Data.Seeders
{
    public class AuthDbSeeder
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthDbSeeder(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            await AddRoles();
            await AddAdmin("admin");
            await AddAdmin("admin2");
            Console.WriteLine("Added");
        }

        private async Task AddRoles()
        {
            foreach (var role in UserRoles.All)
            {
                var roleExists = await _roleManager.RoleExistsAsync(role);
                if (!roleExists)
                    await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        private async Task AddAdmin(string name)
        {
            var newAdminUser = new User()
            {
                UserName = name,
                Email = "moderation@mail.com",
                BlacklistedRefreshTokens = ""
            };

            var existingUser = await _userManager.FindByNameAsync(newAdminUser.UserName);
            if (existingUser == null)
            {
                var createResult = await _userManager.CreateAsync(newAdminUser, "Zagrybas1!");
                if (createResult.Succeeded)
                    await _userManager.AddToRolesAsync(newAdminUser, UserRoles.All);
            }
        }
    }
}
