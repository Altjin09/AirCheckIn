using Air.Core;
using Microsoft.EntityFrameworkCore;

namespace Air.Server.Data;

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }

    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Passenger> Passengers => Set<Passenger>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Flight>().Property(x => x.RowVersion).IsRowVersion();
        b.Entity<Seat>().Property(x => x.RowVersion).IsRowVersion();
        b.Entity<Passenger>().HasIndex(x => x.PassportNo).IsUnique();
        b.Entity<Seat>().HasIndex(x => new { x.FlightId, x.SeatNo }).IsUnique();
    }
}
