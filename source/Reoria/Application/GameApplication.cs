using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reoria.Application.Interfaces;
using Reoria.Application.Networking.Interfaces;

namespace Reoria.Application;

public abstract class GameApplication : IGameApplication
{
    protected readonly ILogger<IGameApplication> logger;
    protected readonly IHostApplicationLifetime appLifetime;
    protected readonly INetworkService networkService;
    protected readonly Timer networkTimer;

    public GameApplication(IServiceProvider services)
    {
        this.logger = services.GetRequiredService<ILogger<IGameApplication>>();
        this.appLifetime = services.GetRequiredService<IHostApplicationLifetime>();
        this.networkService = services.GetRequiredService<INetworkService>();
        this.networkTimer = new(this.NetworkTimerCallback, this, TimeSpan.Zero, TimeSpan.FromMilliseconds(15));
    }

    public abstract Task StartAsync(CancellationToken cancellationToken);
    public abstract Task StopAsync(CancellationToken cancellationToken);

    private void NetworkTimerCallback(object? state) => this.networkService.PollEvents();
}
