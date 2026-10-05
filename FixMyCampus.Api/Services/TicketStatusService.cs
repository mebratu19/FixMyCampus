using FixMyCampus.Api.Models;

namespace FixMyCampus.Api.Services;

public class TicketStatusService
{
    private static readonly Dictionary<string, string> NextStatus =
        new()
        {
            ["New"] = "Assigned",
            ["Assigned"] = "In Progress",
            ["In Progress"] = "Resolved"
        };

    public bool CanMove(string currentStatus, string newStatus)
    {
        return NextStatus.TryGetValue(
            currentStatus,
            out var allowedNext)
            && allowedNext == newStatus;
    }

    public bool TryMove(
        Ticket ticket,
        string newStatus)
    {
        if (!CanMove(ticket.Status, newStatus))
        {
            return false;
        }

        ticket.Status = newStatus;
        return true;
    }
}