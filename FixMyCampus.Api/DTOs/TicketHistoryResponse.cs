namespace FixMyCampus.Api.DTOs;

public record TicketHistoryResponse(
    string? FromStatus,
    string ToStatus,
    string ChangedBy,
    DateTime ChangedAt);