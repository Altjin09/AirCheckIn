using Air.Server.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SeatController : ControllerBase
{
    private readonly SeatService _svc;
    public SeatController(SeatService svc) { _svc = svc; }

    public record LockReq(int flightId, string seatNo, string employeeId);
    public record ConfirmReq(int flightId, string seatNo, int passengerId, string employeeId);

    [HttpPost("lock")]
    public async Task<IActionResult> Lock([FromBody] LockReq req)
    {
        var (ok, err) = await _svc.TryLockSeat(req.flightId, req.seatNo, req.employeeId);
        return ok ? Ok() : Conflict(err);
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmReq req)
    {
        var (ok, err) = await _svc.ConfirmSeat(req.flightId, req.seatNo, req.passengerId, req.employeeId);
        return ok ? Ok() : Conflict(err);
    }
}
