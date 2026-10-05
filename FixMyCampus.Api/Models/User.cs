using Microsoft.AspNetCore.Identity;

namespace FixMyCampus.Api.Models;

public class User : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = "Reporter";

    public ICollection<Ticket> ReportedTickets { get; set; } = new List<Ticket>();

    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

    public ICollection<TicketHistory> TicketHistories { get; set; } = new List<TicketHistory>();
}