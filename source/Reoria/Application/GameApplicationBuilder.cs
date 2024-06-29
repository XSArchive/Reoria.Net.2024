using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reoria.Application.Interfaces;
using Reoria.Application.Networking.Interfaces;
using Reoria.Application.Networking.LiteNetLib;

namespace Reoria.Application;

public abstract class GameApplicationBuilder<TApplicationType>(string[]? args) : IGameApplicationBuilder where TApplicationType : class, IGameApplication
{
    private readonly string[]? args = args;

    public virtual IGameApplicationBuilder CreateApplication()
    {
        IHostBuilder builder = this.OnCreateHostBuilder(Host.CreateDefaultBuilder(this.args));

        IHost app = this.OnBuildApplication(builder.Build());

        app.Run();

        return this;
    }

    protected virtual IHostBuilder OnCreateHostBuilder(IHostBuilder builder) => builder
        .ConfigureAppConfiguration(this.OnConfigureAppConfiguration)
        .ConfigureServices(this.OnConfigureServices)
        .ConfigureLogging(this.ConfigureLogging);

    protected virtual void OnConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder) { }

    protected virtual void OnConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        _ = services.AddSingleton<INetworkService, LiteNetLibNetworkService>();
        _ = services.AddSingleton<IGameApplication, TApplicationType>();
        _ = services.AddHostedService(provider => provider.GetService<IGameApplication>() ?? throw new NullReferenceException());
    }

    protected virtual void ConfigureLogging(HostBuilderContext context, ILoggingBuilder logging) { }

    protected virtual IHost OnBuildApplication(IHost app) => app;
}
