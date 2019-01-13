using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
  //[Authorize]
  [Route("api/camps/{moniker}/speakers")]
  //[ValidateModel]
  public class Speakers2Controller : SpeakersController
  {
    public Speakers2Controller(ICampRepository repo,
          ILogger<SpeakersController> logger, // debate as to whether use Speakers or Speakers2
          IMapper mapper, UserManager<CampUser> userMgr)
      : base(repo, logger, mapper, userMgr) { }

    public override IActionResult Get(string moniker, bool includeTalks = false)
    {  return NotFound(); }

    [ApiVersion("2.0")]
    [Authorize]
    //[AllowAnonymous]
    public override IActionResult GetWithCount(string moniker, bool includeTalks = false)
    {
      var speakers = includeTalks ?
          _repo.GetSpeakersByMonikerWithTalks(moniker) : _repo.GetSpeakersByMoniker(moniker);

      return Ok(new { currentTime = DateTime.UtcNow, count = speakers.Count(),
        results = _mapper.Map<IEnumerable<Speaker2Model>>(speakers)
      });
    }
  }
}
