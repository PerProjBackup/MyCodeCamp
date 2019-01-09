using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using System;
using System.Threading.Tasks;

namespace MyCodeCamp
{
  [Route("api/[controller]")]
  public class CampsController : Controller
  {
    private readonly ICampRepository _repo;
    private readonly ILogger<CampsController> _logger;

    public CampsController(ICampRepository repo, ILogger<CampsController> logger)
    {
      _repo = repo; _logger = logger;
    }

    [HttpGet("")]
    public IActionResult Get()
    {
      var results = _repo.GetAllCamps();

      return Ok(results);
      // new { Name = "Shawn", FavortieColor = "Blue" });
    }

    [HttpGet("{id}", Name = "CampGet")]
    public IActionResult Get(int id, bool includeSpeakers = false)
    {
      try {
        Camp camp = null;

        camp = includeSpeakers ? _repo.GetCampWithSpeakers(id) : _repo.GetCamp(id);

        if (camp != null) return Ok(camp);
        return NotFound($"Camp Id: {id} was not found");
      } catch (Exception) { }
      return BadRequest();
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody]Camp model)
    {
      try {
        _logger.LogInformation("Creating a new Code Camp");
        _repo.Add(model);
        if (await _repo.SaveAllAsync()) {
          var newUri = Url.Link("CampGet", new { model.Id });
          return Created(newUri, model);
        } else _logger.LogWarning("Could not save Camp to the database");
      } catch (Exception ex) {
          _logger.LogError($"Threw exception while saving Camp: {ex}"); }
      return BadRequest();
    }

    //HttpPatch("{id}"), 
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody]Camp model)
    {
      try {
        _logger.LogInformation($"Updating Code Camp ID: {id}");
        var oldCamp = _repo.GetCamp(id);
        if (oldCamp == null) return NotFound($"Could not find a Camp with Id of {id}");

        // Map model to the oldCamp
        oldCamp.Name = model.Name ?? oldCamp.Name;
        oldCamp.Moniker = model.Moniker ?? oldCamp.Moniker;
        oldCamp.Description = model.Description ?? oldCamp.Description;
        oldCamp.Location = model.Location ?? oldCamp.Location;
        oldCamp.Length = model.Length > 0 ? model.Length : oldCamp.Length;
        oldCamp.EventDate = model.EventDate != DateTime.MinValue ? model.EventDate : oldCamp.EventDate;

        if (await _repo.SaveAllAsync()) return Ok(oldCamp);
        else _logger.LogWarning($"Could not update Code Camp ID: {id}");
      } catch (Exception ex) {
        _logger.LogError($"Threw exception while updating Camp ID: {id} {ex}"); }
      return BadRequest($"Could not update Camp ID: {id}"); }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
      try {
        _logger.LogInformation($"Deleting Code Camp ID: {id}");
        var oldCamp = _repo.GetCamp(id);
        if (oldCamp == null) return NotFound($"Could not delete Camp with Id of {id}");
        _repo.Delete(oldCamp);
        if (await _repo.SaveAllAsync()) return Ok();
        else _logger.LogWarning($"Could not delete Code Camp ID: {id}");
      } catch (Exception ex) {
        _logger.LogError($"Threw exception while deleting Camp ID: {id} {ex}"); }
      return BadRequest($"Could not delete Camp ID: {id}");
    }

  }
}
