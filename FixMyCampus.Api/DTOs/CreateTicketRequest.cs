namespace FixMyCampus.Api.DTOs;

public record CreateTicketRequest(
    string Category,
    string Building,
    string Room,
    string Description,
    string Urgency = "Medium");