using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskeen.Application.DTOs;
using Taskeen.Domain.Repositories;

namespace Taskeen.API.Controllers;

[ApiController]
[Route("api/towers")]
[Authorize]
public class TowersController : ControllerBase
{
    private readonly IUnitRepository _unitRepository;

    public TowersController(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TowerDto>>> GetTowers()
    {
        var towers = await _unitRepository.GetAllTowersAsync();
        return Ok(towers.Select(t => new TowerDto(t.Id, t.Name)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TowerDto>> GetTower(int id)
    {
        var tower = await _unitRepository.GetTowerByIdAsync(id);
        if (tower == null) return NotFound();
        return Ok(new TowerDto(tower.Id, tower.Name));
    }
}
