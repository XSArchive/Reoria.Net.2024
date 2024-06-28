using Reoria.Application.Interfaces;
using Reoria.Server.Application;

IGameApplicationBuilder _ = new ServerApplicationBuilder(args).CreateApplication();
