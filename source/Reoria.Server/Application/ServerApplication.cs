using Microsoft.Extensions.Logging;
using Reoria.Application;
using Reoria.Server.Application.Interfaces;

namespace Reoria.Server.Application;

public class ServerApplication(IServiceProvider services) : GameApplication(services), IServerApplication
{
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Run(() => this.logger.LogInformation("Server application initialized."), cancellationToken);
        await Task.Run(() => this.networkService.StartServer(9050, 10, "SomeConnectionKey"), cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.Run(this.networkService.StopServer, cancellationToken);
        await Task.Run(() => this.logger.LogInformation("Server application stopped."), cancellationToken);
    }
}