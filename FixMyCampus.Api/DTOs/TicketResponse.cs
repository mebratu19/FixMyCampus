namespace FixMyCampus.Api.DTOs;

public record TicketResponse(
    int Id,
    string Category,
    string Building,
    string Room,
    string Description,
    string Status,
    string Urgency,
    string ReporterName,
    string? TechnicianName,
    DateTime CreatedAt);