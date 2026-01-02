using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Swagger (API docs UI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "CoachTrack API is running. Use /health or /swagger");
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// --------------------
// In-memory storage (temporary, no database yet)
// --------------------
var clients = new ConcurrentDictionary<Guid, Client>();

// Create a client
app.MapPost("/clients", (CreateClientRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Name))
        return Results.BadRequest(new { error = "Name is required." });

    var client = new Client(
        Id: Guid.NewGuid(),
        Name: req.Name.Trim(),
        StartDate: DateOnly.FromDateTime(DateTime.UtcNow),
        CheckIns: new List<CheckIn>()
    );

    clients[client.Id] = client;
    return Results.Created($"/clients/{client.Id}", client);
});

// List clients
app.MapGet("/clients", () => Results.Ok(clients.Values.OrderByDescending(c => c.StartDate)));

// Add a check-in for a client
app.MapPost("/clients/{clientId:guid}/checkins", (Guid clientId, CreateCheckInRequest req) =>
{
    if (!clients.TryGetValue(clientId, out var client))
        return Results.NotFound(new { error = "Client not found." });

    if (req.WeightKg <= 0)
        return Results.BadRequest(new { error = "WeightKg must be > 0." });

    var checkIn = new CheckIn(
        Id: Guid.NewGuid(),
        Date: DateOnly.FromDateTime(DateTime.UtcNow),
        WeightKg: req.WeightKg,
        Notes: req.Notes?.Trim()
    );

    client.CheckIns.Add(checkIn);
    return Results.Created($"/clients/{clientId}/checkins/{checkIn.Id}", checkIn);
});

// List check-ins for a client
app.MapGet("/clients/{clientId:guid}/checkins", (Guid clientId) =>
{
    if (!clients.TryGetValue(clientId, out var client))
        return Results.NotFound(new { error = "Client not found." });

    return Results.Ok(client.CheckIns.OrderByDescending(c => c.Date));
});

// Get one client
app.MapGet("/clients/{clientId:guid}", (Guid clientId) =>
{
    return clients.TryGetValue(clientId, out var client)
        ? Results.Ok(client)
        : Results.NotFound(new { error = "Client not found." });
});

// Delete one client
app.MapDelete("/clients/{clientId:guid}", (Guid clientId) =>
{
    return clients.TryRemove(clientId, out _)
        ? Results.NoContent()
        : Results.NotFound(new { error = "Client not found." });
});



app.Run();

// --------------------
// Models (kept here for simplicity)
// --------------------
record CreateClientRequest(string Name);
record CreateCheckInRequest(decimal WeightKg, string? Notes);
record Client(Guid Id, string Name, DateOnly StartDate, List<CheckIn> CheckIns);
record CheckIn(Guid Id, DateOnly Date, decimal WeightKg, string? Notes);