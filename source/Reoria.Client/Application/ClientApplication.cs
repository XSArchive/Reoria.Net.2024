using Microsoft.Extensions.Logging;
using Reoria.Application;
using Reoria.Client.Application.Interfaces;

namespace Reoria.Client.Application;

public class ClientApplication(IServiceProvider services) : GameApplication(services), IClientApplication
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Run(() => this.logger.LogInformation("Client application initialized."), cancellationToken);
        await Task.Run(() => this.networkService.StartClient(), cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.Run(this.networkService.StopClient, cancellationToken);
        await Task.Run(() => this.logger.LogInformation("Client application stopped."), cancellationToken);
    }
}