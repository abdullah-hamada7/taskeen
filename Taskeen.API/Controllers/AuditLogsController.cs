using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taskeen.Infrastructure.Persistence;

namespace Taskeen.API.Controllers;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly TaskeenDbContext _context;

    public AuditLogsController(TaskeenDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs()
    {
        var logs = await _context.AuditLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(100)
            .ToListAsync();
        return Ok(logs);
    }

    [HttpGet("entity/{entityName}/{entityId}")]
    public async Task<IActionResult> GetEntityAuditLogs(string entityName, int entityId)
    {
        var logs = await _context.AuditLogs
            .Where(l => l.EntityName == entityName && l.EntityId == entityId)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();
        return Ok(logs);
    }
}
