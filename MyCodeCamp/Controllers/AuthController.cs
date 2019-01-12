using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{
  public class AuthController : Controller
  {
    private CampContext _ctx;
    private SignInManager<CampUser> _signInMgr;
    private readonly ILogger<AuthController> _logger;

    public AuthController(CampContext ctx, SignInManager<CampUser> signInMgr,
          ILogger<AuthController> logger)
    { _ctx = ctx; _signInMgr = signInMgr; _logger = logger; }   

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
  }
}
//private readonly ICampRepository _repo;
//private readonly ILogger<CampsController> _logger;
//private readonly IMapper _mapper;

//public CampsController(ICampRepository repo,
//      ILogger<CampsController> logger, IMapper mapper)
//{
//  _repo = repo; _logger = logger; _mapper = mapper;
//}