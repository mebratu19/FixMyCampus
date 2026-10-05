using FixMyCampus.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FixMyCampus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var openStatuses = new[]
        {
            "New",
            "Assigned",
            "In Progress"
        };

        var openTickets = await _context.Tickets
            .Include(t => t.Reporter)
            .Include(t => t.Technician)
            .Where(t => openStatuses.Contains(t.Status))
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new
            {
                t.Id,
                t.Category,
                t.Building,
                t.Room,
                t.Description,
                t.Status,
                t.Urgency,
                ReporterName = t.Reporter.FullName,
                TechnicianName = t.Technician != null
                    ? t.Technician.FullName
                    : null,
                t.CreatedAt
            })
            .ToListAsync();

        var buildingCounts = await _context.Tickets
            .Where(t => openStatuses.Contains(t.Status))
            .GroupBy(t => t.Building)
            .Select(g => new
            {
                Building = g.Key,
                OpenTickets = g.Count()
            })
            .OrderByDescending(x => x.OpenTickets)
            .ToListAsync();

        return Ok(new
        {
            totalOpenTickets = openTickets.Count,
            openTickets,
            buildingCounts
        });
    }
}