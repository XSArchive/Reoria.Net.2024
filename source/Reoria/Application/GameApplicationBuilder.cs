using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Reoria.Application.Interfaces;
using Reoria.Application.Networking.Interfaces;
using Reoria.Application.Networking.LiteNetLib;

namespace Reoria.Application;

public abstract class GameApplicationBuilder<TApplicationType>(string[]? args) : IGameApplicationBuilder where TApplicationType : class, IGameApplication
{
    private readonly string[]? args = args;

    public virtual IGameApplicationBuilder CreateApplication()
    {
        IHostBuilder builder = Host.CreateDefaultBuilder(this.args);

        _ = builder.ConfigureServices(this.OnConfigureServices);

        IHost app = builder.Build();

        app.Run();

        return this;
    }

    protected virtual void OnConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        _ = services.AddSingleton<INetworkService, LiteNetLibNetworkService>();
        _ = services.AddSingleton<IGameApplication, TApplicationType>();
        _ = services.AddHostedService(provider => provider.GetService<IGameApplication>() ?? throw new NullReferenceException());
    }
}
