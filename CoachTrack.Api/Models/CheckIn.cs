namespace CoachTrack.Api.Models;

public class CheckIn
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }

    public DateTime DateUtc { get; set; }
    public decimal WeightKg { get; set; }
    public string? Notes { get; set; }

    public Client? Client { get; set; }
}
