using Reoria.Application;
using Reoria.Server.Application.Interfaces;

namespace Reoria.Server.Application;

public class ServerApplicationBuilder(string[]? args) : GameApplicationBuilder<ServerApplication>(args), IServerApplicationBuilder
{

}