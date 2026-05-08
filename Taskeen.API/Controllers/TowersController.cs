using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> GetTowers()
    {
        var towers = await _unitRepository.GetAllTowersAsync();
        return Ok(towers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTower(int id)
    {
        var tower = await _unitRepository.GetTowerByIdAsync(id);
        if (tower == null) return NotFound();
        return Ok(tower);
    }
}
