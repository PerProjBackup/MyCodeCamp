﻿using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using MyCodeCamp.Filters;
using MyCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCodeCamp.Controllers
{ 
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  [EnableCors("AnyGet")]
  [Route("api/[controller]")]
  [ValidateModel]
  public class CampsController : BaseController
  {
    private readonly ICampRepository _repo;
    private readonly ILogger<CampsController> _logger;
    private readonly IMapper _mapper;

    public CampsController(ICampRepository repo,
          ILogger<CampsController> logger, IMapper mapper)
    {
      _repo = repo; _logger = logger; _mapper = mapper;
    }

    [HttpGet("")]
    public IActionResult Get()
    {
      var camps = _repo.GetAllCamps();

      return Ok(_mapper.Map<IEnumerable<CampModel>>(camps));
      // new { Name = "Shawn", FavortieColor = "Blue" });
    }

    [HttpGet("{moniker}", Name = "CampGet")]
    public IActionResult Get(string moniker, bool includeSpeakers = false)
    {
      try {
        Camp camp = null;

        camp = includeSpeakers ? _repo.GetCampByMonikerWithSpeakers(moniker) : _repo.GetCampByMoniker(moniker);

        if (camp != null) return Ok(_mapper.Map<CampModel>(camp));
            // _mapper.Map<CampModel>(camp, opt => opt.Items["UrlHelper"] = Url));
      
        return NotFound($"Camp Moniker: {moniker} was not found");
      } catch (Exception) { }
      return BadRequest();
    }

    [EnableCors("Wildermuth")]
    [Authorize(Policy = "SuperUsers")]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody]CampModel model)
    {
      try {
        _logger.LogInformation("Creating a new Code Camp");
        var camp = _mapper.Map<Camp>(model);

        _repo.Add(camp);
        if (await _repo.SaveAllAsync()) {
          var newUri = Url.Link("CampGet", new { moniker = camp.Moniker });
          return Created(newUri, _mapper.Map<CampModel>(camp));
        } else _logger.LogWarning("Could not save Camp to the database");
      } catch (Exception ex) {
          _logger.LogError($"Threw exception while saving Camp: {ex}"); }
      return BadRequest();
    }

    //HttpPatch("{id}"), 
    [HttpPut("{moniker}")]
    public async Task<IActionResult> Put(string moniker, [FromBody]CampModel model)
    {
      try {
        _logger.LogInformation($"Updating Code Camp ID: {moniker}");
        var oldCamp = _repo.GetCampByMoniker(moniker);
        if (oldCamp == null) return NotFound($"Could not find a Camp with Id {moniker}");

        _mapper.Map(model, oldCamp);
        //// Map model to the oldCamp
        //oldCamp.Name = model.Name ?? oldCamp.Name;
        //oldCamp.Moniker = model.Moniker ?? oldCamp.Moniker;
        //oldCamp.Description = model.Description ?? oldCamp.Description;
        //oldCamp.Location = model.Location ?? oldCamp.Location;
        //oldCamp.Length = model.Length > 0 ? model.Length : oldCamp.Length;
        //oldCamp.EventDate = model.EventDate != DateTime.MinValue ? model.EventDate : oldCamp.EventDate;

        if (await _repo.SaveAllAsync()) return Ok(_mapper.Map<CampModel>(oldCamp));
        else _logger.LogWarning($"Could not update Code Camp ID: {moniker}");
      } catch (Exception ex) {
        _logger.LogError($"Threw exception while updating Camp ID: {moniker} {ex}"); }
      return BadRequest($"Could not update Camp ID: {moniker}"); }

    [HttpDelete("{moniker}")]
    public async Task<IActionResult> Delete(string moniker)
    {
      try {
        _logger.LogInformation($"Deleting Code Camp ID: {moniker}");
        var oldCamp = _repo.GetCampByMoniker(moniker);
        if (oldCamp == null) return NotFound($"Could not delete Camp with Id of {moniker}");
        _repo.Delete(oldCamp);
        if (await _repo.SaveAllAsync()) return Ok();
        else _logger.LogWarning($"Could not delete Code Camp ID: {moniker}");
      } catch (Exception ex) {
        _logger.LogError($"Threw exception while deleting Camp ID: {moniker} {ex}"); }
      return BadRequest($"Could not delete Camp ID: {moniker}");
    }

  }
}
