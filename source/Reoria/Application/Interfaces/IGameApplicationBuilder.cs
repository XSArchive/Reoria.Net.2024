namespace Reoria.Application.Interfaces;

public interface IGameApplicationBuilder
{
    IGameApplicationBuilder CreateApplication<TApplicationType>() where TApplicationType : class, IGameApplication;
}