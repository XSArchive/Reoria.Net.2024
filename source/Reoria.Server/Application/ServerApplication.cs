using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reoria.Application;
using Reoria.Server.Application.Interfaces;

namespace Reoria.Server.Application;

public class ServerApplication(ILogger<ServerApplication> logger, IHostApplicationLifetime appLifetime) : GameApplication(logger, appLifetime), IServerApplication
{
    public override async Task StartAsync(CancellationToken cancellationToken) => await Task.Run(() => this.logger.LogInformation("Server application initialized."), cancellationToken);
    public override async Task StopAsync(CancellationToken cancellationToken) => await Task.Run(() => this.logger.LogInformation("Server application stopped."), cancellationToken);
}