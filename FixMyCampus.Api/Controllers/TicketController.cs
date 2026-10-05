using FixMyCampus.Api.Data;
using FixMyCampus.Api.DTOs;
using FixMyCampus.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FixMyCampus.Api.Services;
namespace FixMyCampus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketController : ControllerBase
{
    private readonly AppDbContext _context;

    public TicketController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketRequest request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var ticket = new Ticket
        {
            Category = request.Category,
            Building = request.Building,
            Room = request.Room,
            Description = request.Description,
            Urgency = request.Urgency,
            Status = "New",
            ReporterId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);

        await _context.SaveChangesAsync();

        var history = new TicketHistory
        {
            TicketId = ticket.Id,
            FromStatus = null,
            ToStatus = "New",
            ChangedById = userId,
            ChangedAt = DateTime.UtcNow
        };

        _context.TicketHistories.Add(history);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = ticket.Id },
            new TicketResponse(
                ticket.Id,
                ticket.Category,
                ticket.Building,
                ticket.Room,
                ticket.Description,
                ticket.Status,
                ticket.Urgency,
                User.Identity?.Name ?? "Reporter",
                null,
                ticket.CreatedAt));
    }
[HttpPut("{id:int}/status")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> UpdateStatus(
    int id,
    UpdateTicketStatusRequest request,
    [FromServices] TicketStatusService statusService)
{
    var ticket = await _context.Tickets
        .FirstOrDefaultAsync(t => t.Id == id);

    if (ticket == null)
    {
        return NotFound(new
        {
            message = "Ticket not found."
        });
    }

    var userIdValue = User.FindFirstValue(
        ClaimTypes.NameIdentifier);

    if (!int.TryParse(userIdValue, out var userId))
    {
        return Unauthorized();
    }

    var oldStatus = ticket.Status;

    if (!statusService.TryMove(ticket, request.Status))
    {
        return BadRequest(new
        {
            message =
                $"Invalid status transition: {oldStatus} -> {request.Status}.",
            allowedFlow =
                "New -> Assigned -> In Progress -> Resolved"
        });
    }

    var history = new TicketHistory
    {
        TicketId = ticket.Id,
        FromStatus = oldStatus,
        ToStatus = ticket.Status,
        ChangedById = userId,
        ChangedAt = DateTime.UtcNow
    };

    _context.TicketHistories.Add(history);

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Ticket status updated successfully.",
        ticketId = ticket.Id,
        fromStatus = oldStatus,
        toStatus = ticket.Status
    });
}
[HttpPut("{id:int}/assign")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> AssignTechnician(
    int id,
    AssignTicketRequest request)
{
    var ticket = await _context.Tickets
        .FirstOrDefaultAsync(t => t.Id == id);

    if (ticket == null)
    {
        return NotFound(new
        {
            message = "Ticket not found."
        });
    }

    var technician = await _context.Users
        .FirstOrDefaultAsync(u =>
            u.FullName == request.TechnicianName);

    if (technician == null)
    {
        return NotFound(new
        {
            message = "Technician not found."
        });
    }

    if (ticket.Status != "New")
    {
        return BadRequest(new
        {
            message =
                "Only New tickets can be assigned."
        });
    }

    ticket.TechnicianId = technician.Id;

    var oldStatus = ticket.Status;

    ticket.Status = "Assigned";

    var userIdValue = User.FindFirstValue(
        ClaimTypes.NameIdentifier);

    if (!int.TryParse(userIdValue, out var userId))
    {
        return Unauthorized();
    }

    var history = new TicketHistory
    {
        TicketId = ticket.Id,
        FromStatus = oldStatus,
        ToStatus = "Assigned",
        ChangedById = userId,
        ChangedAt = DateTime.UtcNow
    };

    _context.TicketHistories.Add(history);

    await _context.SaveChangesAsync();

    return Ok(new
    {
        message = "Ticket assigned successfully.",
        ticketId = ticket.Id,
        technician = technician.FullName,
        fromStatus = oldStatus,
        toStatus = ticket.Status
    });
}
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? building,
        [FromQuery] string? status)
    {
        var query = _context.Tickets
            .Include(t => t.Reporter)
            .Include(t => t.Technician)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(building))
        {
            query = query.Where(t => t.Building == building);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(t => t.Status == status);
        }

        var tickets = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketResponse(
                t.Id,
                t.Category,
                t.Building,
                t.Room,
                t.Description,
                t.Status,
                t.Urgency,
                t.Reporter.FullName,
                t.Technician != null
                    ? t.Technician.FullName
                    : null,
                t.CreatedAt))
            .ToListAsync();

        return Ok(tickets);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTickets()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var tickets = await _context.Tickets
            .Include(t => t.Reporter)
            .Include(t => t.Technician)
            .Where(t => t.ReporterId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketResponse(
                t.Id,
                t.Category,
                t.Building,
                t.Room,
                t.Description,
                t.Status,
                t.Urgency,
                t.Reporter.FullName,
                t.Technician != null
                    ? t.Technician.FullName
                    : null,
                t.CreatedAt))
            .ToListAsync();

        return Ok(tickets);
    }

    [HttpGet("{id:int}")]
public async Task<IActionResult> GetById(int id)
{
    var ticket = await _context.Tickets
        .Include(t => t.Reporter)
        .Include(t => t.Technician)
        .Include(t => t.History)
            .ThenInclude(h => h.ChangedBy)
        .FirstOrDefaultAsync(t => t.Id == id);

    if (ticket == null)
    {
        return NotFound(new
        {
            message = "Ticket not found."
        });
    }

    var response = new
    {
        ticket.Id,
        ticket.Category,
        ticket.Building,
        ticket.Room,
        ticket.Description,
        ticket.Status,
        ticket.Urgency,

        ReporterName = ticket.Reporter.FullName,

        TechnicianName = ticket.Technician?.FullName,

        ticket.CreatedAt,

        History = ticket.History
            .OrderBy(h => h.ChangedAt)
            .Select(h => new TicketHistoryResponse(
                h.FromStatus,
                h.ToStatus,
                h.ChangedBy.FullName,
                h.ChangedAt))
            .ToList()
    };

    return Ok(response);
}
}