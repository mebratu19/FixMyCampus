namespace FixMyCampus.Api.Models;

public class Ticket
{
    public int Id { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Building { get; set; } = string.Empty;

    public string Room { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = "New";

    public string Urgency { get; set; } = "Medium";

    public int ReporterId { get; set; }

    public User Reporter { get; set; } = null!;

    public int? TechnicianId { get; set; }

    public User? Technician { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
}