using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using System;

namespace MyCodeCamp
{
  [Route("api/[controller]")]
  public class CampsController : Controller
  {
    private readonly ICampRepository _repo;

    public CampsController(ICampRepository repo)
    {
      _repo = repo;
    }

    [HttpGet("")]
    public IActionResult Get()
    {
      var results = _repo.GetAllCamps();

      return Ok(results);
      // new { Name = "Shawn", FavortieColor = "Blue" });
    }

    [HttpGet("{id}")]
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
  }
}
