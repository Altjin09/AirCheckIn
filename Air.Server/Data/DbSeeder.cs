using Air.Core;
using Microsoft.EntityFrameworkCore;

namespace Air.Server.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDb db)
    {
        if (await db.Flights.AnyAsync()) return;

        var f = new Flight
        {
            FlightNo = "HK4701",
            From = "Ulaanbaatar",
            To = "Hong-Kong",
            ScheduledDeparture = DateTime.Today.AddHours(12),
            Gate = "A56"
        };
        // 30 мөр * A..F
        for (int r = 1; r <= 30; r++)
            foreach (var c in new[] { 'A', 'B', 'C', 'D', 'E', 'F' })
                f.Seats.Add(new Seat { SeatNo = $"{r}{c}" });

        var p1 = new Passenger { FullName = "Altjin Batsaikhan", PassportNo = "AB1234567" };
        var p2 = new Passenger { FullName = "Tugs Erdene", PassportNo = "CD7654321" };
        var b1 = new Booking { Flight = f, Passenger = p1 };
        var b2 = new Booking { Flight = f, Passenger = p2 };

        db.AddRange(f, p1, p2, b1, b2);
        await db.SaveChangesAsync();
    }
}
