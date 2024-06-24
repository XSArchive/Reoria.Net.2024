using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reoria.Application.Interfaces;

namespace Reoria.Application;

public abstract class GameApplication(ILogger<IGameApplication> logger, IHostApplicationLifetime appLifetime) : IGameApplication
{
    protected readonly ILogger<IGameApplication> logger = logger;
    protected readonly IHostApplicationLifetime appLifetime = appLifetime;

    public abstract Task StartAsync(CancellationToken cancellationToken);
    public abstract Task StopAsync(CancellationToken cancellationToken);
}
