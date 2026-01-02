using CoachTrack.Api.Data;
using CoachTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB
var cs = builder.Configuration.GetConnectionString("CoachTrackDb");
builder.Services.AddDbContext<CoachTrackDbContext>(opt => opt.UseNpgsql(cs));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "CoachTrack API is running. Use /swagger");
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Create client
app.MapPost("/clients", async (CoachTrackDbContext db, CreateClientRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Name))
        return Results.BadRequest(new { error = "Name is required." });

    var client = new Client
    {
        Id = Guid.NewGuid(),
        Name = req.Name.Trim(),
        StartDateUtc = DateTime.UtcNow
    };

    db.Clients.Add(client);
    await db.SaveChangesAsync();

    return Results.Created($"/clients/{client.Id}", client);
});

// List clients
app.MapGet("/clients", async (CoachTrackDbContext db) =>
{
    var clients = await db.Clients
        .OrderByDescending(c => c.StartDateUtc)
        .ToListAsync();

    return Results.Ok(clients);
});

// Get single client (with check-ins)
app.MapGet("/clients/{clientId:guid}", async (CoachTrackDbContext db, Guid clientId) =>
{
    var client = await db.Clients
        .Include(c => c.CheckIns)
        .FirstOrDefaultAsync(c => c.Id == clientId);

    return client is null
        ? Results.NotFound(new { error = "Client not found." })
        : Results.Ok(client);
});

// Delete client
app.MapDelete("/clients/{clientId:guid}", async (CoachTrackDbContext db, Guid clientId) =>
{
    var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == clientId);
    if (client is null)
        return Results.NotFound(new { error = "Client not found." });

    db.Clients.Remove(client);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

// Add check-in
app.MapPost("/clients/{clientId:guid}/checkins", async (CoachTrackDbContext db, Guid clientId, CreateCheckInRequest req) =>
{
    var clientExists = await db.Clients.AnyAsync(c => c.Id == clientId);
    if (!clientExists)
        return Results.NotFound(new { error = "Client not found." });

    if (req.WeightKg <= 0)
        return Results.BadRequest(new { error = "WeightKg must be > 0." });

    var checkIn = new CheckIn
    {
        Id = Guid.NewGuid(),
        ClientId = clientId,
        DateUtc = DateTime.UtcNow,
        WeightKg = req.WeightKg,
        Notes = req.Notes?.Trim()
    };

    db.CheckIns.Add(checkIn);
    await db.SaveChangesAsync();

    return Results.Created($"/clients/{clientId}/checkins/{checkIn.Id}", checkIn);
});

// List check-ins
app.MapGet("/clients/{clientId:guid}/checkins", async (CoachTrackDbContext db, Guid clientId) =>
{
    var clientExists = await db.Clients.AnyAsync(c => c.Id == clientId);
    if (!clientExists)
        return Results.NotFound(new { error = "Client not found." });

    var checkIns = await db.CheckIns
        .Where(ci => ci.ClientId == clientId)
        .OrderByDescending(ci => ci.DateUtc)
        .ToListAsync();

    return Results.Ok(checkIns);
});

app.Run();

record CreateClientRequest(string Name);
record CreateCheckInRequest(decimal WeightKg, string? Notes);
