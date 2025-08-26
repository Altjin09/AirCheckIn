using System.Net;
using System.Net.Sockets;
using System.Text;
using Air.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Air.Server.Services;
public class TcpServerService : BackgroundService
{
    private readonly IServiceScopeFactory _scope;
    public TcpServerService(IServiceScopeFactory s) => _scope = s;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var listener = new TcpListener(IPAddress.Any, 9020);
        listener.Start();
        while (!ct.IsCancellationRequested)
        {
            var client = await listener.AcceptTcpClientAsync(ct);
            _ = Task.Run(() => Handle(client, ct), ct);
        }
    }

    private async Task Handle(TcpClient client, CancellationToken ct)
    {
        using var scope = _scope.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDb>();
        using var stream = client.GetStream();
        var buf = new byte[4096];
        var len = await stream.ReadAsync(buf, ct);
        var req = Encoding.UTF8.GetString(buf, 0, len).Trim();
        string resp = "ERR";

        // Протокол (жишээ): "STATUS HK4701" эсвэл "CHECK HK4701 AB1234567"
        var parts = req.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2 && parts[0].Equals("STATUS", StringComparison.OrdinalIgnoreCase))
        {
            var f = await db.Flights.FirstOrDefaultAsync(x => x.FlightNo == parts[1], ct);
            resp = f is null ? "NOTFOUND" : f.Status.ToString().ToUpperInvariant();
        }
        else if (parts.Length >= 3 && parts[0].Equals("CHECK", StringComparison.OrdinalIgnoreCase))
        {
            var f = await db.Flights.FirstOrDefaultAsync(x => x.FlightNo == parts[1], ct);
            var pass = await db.Passengers.FirstOrDefaultAsync(x => x.PassportNo == parts[2], ct);
            resp = (f == null || pass == null) ? "NOTFOUND" : "OK";
        }

        var bytes = Encoding.UTF8.GetBytes(resp);
        await stream.WriteAsync(bytes, 0, bytes.Length, ct);
        client.Close();
    }
}
