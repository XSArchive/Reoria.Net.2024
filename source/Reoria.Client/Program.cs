using Reoria.Application.Interfaces;
using Reoria.Client.Application;

IGameApplicationBuilder _ = new ClientApplicationBuilder(args).CreateApplication<ClientApplication>();
