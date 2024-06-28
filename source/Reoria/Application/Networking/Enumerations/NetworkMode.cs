namespace Reoria.Application.Networking.Enumerations;

public enum NetworkMode : byte
{
    None = 1 << 0,

    Client = 1 << 1,
    Server = 1 << 2,

    All = None | Client | Server
}
