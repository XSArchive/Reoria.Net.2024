using Reoria.Application.Networking.Interfaces;

namespace Reoria.Application.Networking.Enumerations;

/// <summary>
/// Defines the application modes that a <see cref="INetworkService"/> can run as.
/// </summary>
public enum NetworkMode : byte
{
    /// <summary>
    /// The <see cref="INetworkService"/> is currently not running.
    /// </summary>
    None = 1 << 0,

    /// <summary>
    /// The <see cref="INetworkService"/> is currently running as a client.
    /// </summary>
    Client = 1 << 1,
    /// <summary>
    /// The <see cref="INetworkService"/> is currently running as a server.
    /// </summary>
    Server = 1 << 2,

    /// <summary>
    /// Ignores what the <see cref="INetworkService"/> is currently running as.
    /// </summary>
    All = None | Client | Server
}
