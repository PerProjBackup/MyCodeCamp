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
  [Authorize]
  [Route("api/camps/{moniker}/speakers")]
  [ValidateModel]
  [ApiVersion("1.0")]
  [ApiVersion("1.1")]
  public class SpeakersController : BaseController
  {
    protected readonly ICampRepository _repo;
    protected readonly ILogger<SpeakersController> _logger;
    protected readonly IMapper _mapper;
    protected UserManager<CampUser> _userMgr;

    public SpeakersController(ICampRepository repo,
          ILogger<SpeakersController> logger,
          IMapper mapper, UserManager<CampUser> userMgr) {
      _repo = repo; _logger = logger;
      _mapper = mapper; _userMgr = userMgr;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [AllowAnonymous]
    public virtual IActionResult Get(string moniker, bool includeTalks = false)
    {
      var speakers = includeTalks ?
          _repo.GetSpeakersByMonikerWithTalks(moniker) : _repo.GetSpeakersByMoniker(moniker);

      return Ok(_mapper.Map<IEnumerable<SpeakerModel>>(speakers));
    }

    [HttpGet]
    [MapToApiVersion("1.1")]
    [AllowAnonymous]
    public virtual IActionResult GetWithCount(string moniker, bool includeTalks = false)
    {
      var speakers = includeTalks ?
          _repo.GetSpeakersByMonikerWithTalks(moniker) : _repo.GetSpeakersByMoniker(moniker);

      return Ok(new { count = speakers.Count(),
        results = _mapper.Map<IEnumerable<SpeakerModel>>(speakers)});
    }

    [HttpGet("{id}", Name ="SpeakerGet")]
    //[AllowAnonymous]
    public IActionResult Get(string moniker, int id, bool includeTalks = false)
    {
      var speaker = includeTalks ? _repo.GetSpeakerWithTalks(id) : _repo.GetSpeaker(id);
      if (speaker == null) return NotFound();
      if (speaker.Camp.Moniker != moniker) return BadRequest("Speaker not in specified Camp");

      return Ok(_mapper.Map<SpeakerModel>(speaker));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Post(string moniker, [FromBody]SpeakerModel model) {
      try {        
        _logger.LogInformation($"Creating a new Speaker for Code Camp ID: {moniker}");
        var camp = _repo.GetCampByMoniker(moniker);
        if (camp == null) return NotFound($"Could not find a Camp with Id: {moniker}");

        var speaker = _mapper.Map<Speaker>(model);
        speaker.Camp = camp;

        var campUser = await _userMgr.FindByNameAsync(User.Identity.Name);
        if (campUser != null) { speaker.User = campUser;

          _repo.Add(speaker);
          if (await _repo.SaveAllAsync()){
            var url = Url.Link("SpeakerGet",
                    new { moniker = camp.Moniker, id = speaker.Id });
            return Created(url, _mapper.Map<SpeakerModel>(speaker));
          } else _logger.LogWarning($"Could not add new Speaker for Code Camp ID: {moniker}");
        }
      }
      catch (Exception ex) { _logger.LogError
            ($"Exception thrown while adding Speaker for Camp ID: {moniker}, {ex}");
      }           
      return BadRequest($"Could not add new Speaker for Camp ID: {moniker}");
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Put(string moniker,
                              int id, [FromBody]SpeakerModel model)
    {
      try {
        _logger.LogInformation($"Updating Speaker for Code Camp ID: {moniker}");
        var oldSpeaker = _repo.GetSpeaker(id);
        if (oldSpeaker == null)
          return NotFound($"Could not find a Speaker with Camp Id: {moniker}");
        if (oldSpeaker.Camp.Moniker != moniker) return BadRequest(
           $"Speaker with ID: \"{id}\" does not match Camp ID: \"{moniker}\"");
        if (oldSpeaker.User.UserName == User.Identity.Name) return Forbid();

        _mapper.Map(model, oldSpeaker);        
        if (await _repo.SaveAllAsync()) return Ok(_mapper.Map<SpeakerModel>(oldSpeaker));
        else _logger.LogWarning($"Could not update Code Camp ID: {moniker}");
      } catch (Exception ex) { _logger.LogError(
          $"Threw exception while updating Speaker with ID: {id} and Camp ID: {moniker} {ex}"); }
      return BadRequest(
          $"Could not update Speaker with ID: {id} and Camp ID: {moniker}"); }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(string moniker, int id)
    {
      try {
        _logger.LogInformation($"Deleting Speaker ID: {id}");
        var oldSpeaker = _repo.GetSpeaker(id);
        if (oldSpeaker == null) return NotFound(
            $"Could not find a Speaker with Camp Id: {moniker}");
        if (oldSpeaker.Camp.Moniker != moniker) return BadRequest(
            $"Speaker with ID: \"{id}\" does not match Camp ID: \"{moniker}\"");
        if (oldSpeaker.User.UserName == User.Identity.Name) return Forbid();

        _repo.Delete(oldSpeaker);
        if (await _repo.SaveAllAsync()) return Ok();
        else _logger.LogWarning($"Could not delete Speaker ID: {id}");
      } catch (Exception ex) { _logger.LogError(
            $"Threw exception while deleting Speaker ID: {id} {ex}"); }
      return BadRequest($"Could not delete Speaker ID: {id}");
    }

  }
}
