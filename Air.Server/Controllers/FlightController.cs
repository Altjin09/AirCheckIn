using Air.Core;
using Air.Server.Data;
using Air.Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class FlightController : ControllerBase
{
    private readonly AppDb _db;
    private readonly IHubContext<FlightsHub> _hub;
    public FlightController(AppDb db, IHubContext<FlightsHub> hub) { _db = db; _hub = hub; }

    [HttpGet("list")]
    public async Task<IEnumerable<object>> List() =>
        await _db.Flights.OrderBy(x => x.ScheduledDeparture)
        .Select(x => new { x.Id, x.FlightNo, x.From, x.To, x.Gate, x.Status, x.ScheduledDeparture })
        .ToListAsync();

    [HttpGet("{id}")]
    public Task<Flight?> Get(int id) => _db.Flights.Include(f => f.Seats).FirstOrDefaultAsync(f => f.Id == id);

    [HttpGet("{id}/seats")]
    public async Task<IEnumerable<object>> Seats(int id) =>
        await _db.Seats.Where(s => s.FlightId == id)
           .Select(s => new { s.SeatNo, s.IsAssigned, s.LockedUntilUtc, s.LockedBy, s.AssignedPassengerId })
           .ToListAsync();

    [HttpPut("status")]
    public async Task<IActionResult> SetStatus([FromBody] dynamic body)
    {
        int flightId = (int)body.flightId;
        string statusStr = (string)body.status;
        var f = await _db.Flights.FindAsync(flightId);
        if (f == null) return NotFound();
        f.Status = Enum.Parse<FlightStatus>(statusStr, ignoreCase: true);
        await _db.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("FlightStatusChanged", flightId, f.Status.ToString());
        return Ok();
    }
}
