using Microsoft.AspNetCore.Identity;
using MyCodeCamp.Data.Entities;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyCodeCamp.Data
{
  public class CampIdentitySeeder
  {
    private RoleManager<IdentityRole> _roleMgr;
    private UserManager<CampUser> _userMgr;

    public CampIdentitySeeder(UserManager<CampUser> userMgr, RoleManager<IdentityRole> roleMgr)
    {
      _userMgr = userMgr;
      _roleMgr = roleMgr;
    }

    public async Task SeedAsync()
    {
      var user = await _userMgr.FindByNameAsync("shawnwildermuth");

      // Add User
      if (user == null)
      {
        if (!(await _roleMgr.RoleExistsAsync("Admin")))
        {
          var role = new IdentityRole("Admin");
          await _roleMgr.CreateAsync(role);
          await _roleMgr.AddClaimAsync(role, new Claim(type: "IsAdmin", value: "True"));
        }

        user = new CampUser()
        {
          UserName = "shawnwildermuth",
          FirstName = "Shawn",
          LastName = "Wildermuth",
          Email = "shawn@wildermuth.com"
        };
      
        var userResult = await _userMgr.CreateAsync(user, "P@$$w0rd");
        var roleResult = await _userMgr.AddToRoleAsync(user, "Admin");
        var claimResult = await _userMgr.AddClaimAsync(user, new Claim("SuperUser", "True"));

        if (!userResult.Succeeded || !roleResult.Succeeded || !claimResult.Succeeded)
        {
          throw new InvalidOperationException("Failed to build user and roles");
        }

      }
    }
  }
}
