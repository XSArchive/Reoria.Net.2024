using Reoria.Application;
using Reoria.Client.Application.Interfaces;

namespace Reoria.Client.Application;

public class ClientApplicationBuilder(string[]? args) : GameApplicationBuilder(args), IClientApplicationBuilder
{

}