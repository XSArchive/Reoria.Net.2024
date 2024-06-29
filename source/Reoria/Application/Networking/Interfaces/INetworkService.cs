namespace Reoria.Application.Networking.Interfaces;

public interface INetworkService
{
    void PollEvents();
    void StartClient(string ipAddress, int port, string connectionKey);
    void StartServer(int port, int maxConnections, string connectionKey);
    void Stop();
}
