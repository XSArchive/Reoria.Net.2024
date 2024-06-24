using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reoria.Application;
using Reoria.Client.Application.Interfaces;

namespace Reoria.Client.Application;

public class ClientApplication(ILogger<ClientApplication> logger, IHostApplicationLifetime appLifetime) : GameApplication(logger, appLifetime), IClientApplication
{
    public override async Task StartAsync(CancellationToken cancellationToken) => await Task.Run(() => this.logger.LogInformation("Client application initialized."), cancellationToken);
    public override async Task StopAsync(CancellationToken cancellationToken) => await Task.Run(() => this.logger.LogInformation("Client application stopped."), cancellationToken);
}