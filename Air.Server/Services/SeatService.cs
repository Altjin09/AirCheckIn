using Air.Core;
using Air.Server.Data;
using Air.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Air.Server.Services;

public class SeatService
{
    private readonly AppDb _db;
    private readonly IHubContext<FlightsHub> _hub;
    public SeatService(AppDb db, IHubContext<FlightsHub> hub) { _db = db; _hub = hub; }

    public async Task<(bool ok, string? err)> TryLockSeat(int flightId, string seatNo, string employeeId, int seconds = 45)
    {
        var seat = await _db.Seats.FirstOrDefaultAsync(s => s.FlightId == flightId && s.SeatNo == seatNo);
        if (seat == null) return (false, "Seat not found");
        if (seat.IsAssigned) return (false, "Seat already assigned");
        if (seat.LockedUntilUtc is not null && seat.LockedUntilUtc > DateTime.UtcNow) return (false, "Seat locked");

        seat.LockedBy = employeeId;
        seat.LockedUntilUtc = DateTime.UtcNow.AddSeconds(seconds);
        await _db.SaveChangesAsync();

        await _hub.Clients.Group($"flight-{flightId}")
            .SendAsync("SeatLocked", flightId, seatNo, employeeId, seat.LockedUntilUtc);
        return (true, null);
    }

    public async Task<(bool ok, string? err)> ConfirmSeat(int flightId, string seatNo, int passengerId, string employeeId)
    {
        var seat = await _db.Seats.FirstAsync(s => s.FlightId == flightId && s.SeatNo == seatNo);
        if (seat.IsAssigned) return (false, "Already assigned");
        if (!(seat.LockedBy == employeeId && seat.LockedUntilUtc > DateTime.UtcNow))
            return (false, "Seat not locked by you");

        seat.IsAssigned = true;
        seat.AssignedPassengerId = passengerId;
        seat.LockedBy = null; seat.LockedUntilUtc = null;

        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.FlightId == flightId && b.PassengerId == passengerId);
        if (booking != null) { booking.CheckedIn = true; booking.CheckedInAtUtc = DateTime.UtcNow; }

        await _db.SaveChangesAsync();
        await _hub.Clients.Group($"flight-{flightId}")
            .SendAsync("SeatAssigned", flightId, seatNo, passengerId);
        return (true, null);
    }

    public async Task ReleaseExpiredLocks()
    {
        var now = DateTime.UtcNow;
        var expired = await _db.Seats.Where(s => s.LockedUntilUtc != null && s.LockedUntilUtc < now).ToListAsync();
        if (expired.Count == 0) return;
        foreach (var s in expired) { s.LockedBy = null; s.LockedUntilUtc = null; }
        await _db.SaveChangesAsync();
        foreach (var s in expired)
            await _hub.Clients.Group($"flight-{s.FlightId}")
                .SendAsync("SeatReleased", s.FlightId, s.SeatNo);
    }
}
