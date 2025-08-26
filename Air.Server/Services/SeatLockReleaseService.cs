using Microsoft.Extensions.Hosting;

namespace Air.Server.Services;
public class SeatLockReleaseService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    public SeatLockReleaseService(IServiceScopeFactory f) => _scopeFactory = f;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<SeatService>();
            await service.ReleaseExpiredLocks();
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }
    }
}
