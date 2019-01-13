using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
  public class AuthController : Controller
  {
    private CampContext _ctx;
    private SignInManager<CampUser> _signInMgr;
    private readonly ILogger<AuthController> _logger;
    private UserManager<CampUser> _userMgr;
    private IConfiguration _config;

    public AuthController(CampContext ctx, SignInManager<CampUser> signInMgr,
          ILogger<AuthController> logger, UserManager<CampUser> userMgr,
          IConfiguration config)
    { _ctx = ctx; _signInMgr = signInMgr; _logger = logger;
      _userMgr = userMgr; _config = config;  }   

    [HttpPost("api/auth/login")]
    [ValidateModel]
    public async Task<IActionResult> Login([FromBody]CredentialModel model)
    {
      try { var result = await _signInMgr.PasswordSignInAsync(
          model.UserName, model.Password, false, false);
        if (result.Succeeded) return Ok(); 
      } catch (Exception ex) {
          _logger.LogError($"Exception thrown while while logging in: {ex}"); }
      return BadRequest("Failed to login");
    }

    [HttpPost("api/auth/token")]
    [ValidateModel]
    public async Task<IActionResult> CreateToken([FromBody]CredentialModel model)
    {
      try { var user = await _userMgr.FindByNameAsync(model.UserName);
        if (user != null) { 
          var result = await _signInMgr.CheckPasswordSignInAsync(user, model.Password, false);
          if (result.Succeeded) {
            var userClaims = await _userMgr.GetClaimsAsync(user);
            var claims = new[] { new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
              new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
              new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
              new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
              new Claim(JwtRegisteredClaimNames.Sub, user.Email) }.Union(userClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer: _config["Tokens:Issuer"],
              audience: _config["Tokens:Audience"], claims: claims,
              expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Tokens:Expires"])),
              signingCredentials: creds);
            var tokenResults = new { token = new JwtSecurityTokenHandler().WriteToken(token),
              expiration = token.ValidTo };
            return Created("", tokenResults); } }
      } catch (Exception ex) {
          _logger.LogError($"Exception thrown while while creating JWT: {ex}"); }
      return BadRequest("Failed to generate token");

    }
  }
}
