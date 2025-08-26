using Air.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly AppDb _db;
    public BookingController(AppDb db) { _db = db; }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string passport, [FromQuery] string flightNo)
    {
        var q = from b in _db.Bookings.Include(b => b.Passenger).Include(b => b.Flight)
                where b.Passenger.PassportNo == passport && b.Flight.FlightNo == flightNo
                select new { b.Id, Passenger = b.Passenger.FullName, b.Passenger.PassportNo, b.Flight.Id, b.Flight.FlightNo };
        var r = await q.FirstOrDefaultAsync();
        return r == null ? NotFound() : Ok(r);
    }
}
