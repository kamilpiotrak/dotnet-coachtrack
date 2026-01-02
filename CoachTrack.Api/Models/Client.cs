namespace CoachTrack.Api.Models;

public class Client
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime StartDateUtc { get; set; }

    public List<CheckIn> CheckIns { get; set; } = new();
}
