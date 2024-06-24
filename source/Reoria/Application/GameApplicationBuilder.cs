using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Reoria.Application.Interfaces;

namespace Reoria.Application;

public abstract class GameApplicationBuilder(string[]? args) : IGameApplicationBuilder
{
    private readonly string[]? args = args;

    public virtual IGameApplicationBuilder CreateApplication<TApplicationType>() where TApplicationType : class, IGameApplication
    {
        IHostBuilder builder = Host.CreateDefaultBuilder(this.args)
        .ConfigureServices((hostContext, services) =>
        {
            _ = services.AddSingleton<IGameApplication, TApplicationType>();
            _ = services.AddHostedService(provider => provider.GetService<IGameApplication>() ?? throw new NullReferenceException());
        });

        IHost app = builder.Build();

        app.Run();

        return this;
    }
}
