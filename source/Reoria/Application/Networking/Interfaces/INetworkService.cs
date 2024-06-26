namespace Reoria.Application.Networking.Interfaces;

public interface INetworkService
{
    void PollEvents();
    void StartClient(string ipAddress = "localhost", int port = 9050, string connectionKey = "SomeConnectionKey");
    void StartServer(int port = 9050, int maxConnections = 10, string connectionKey = "SomeConnectionKey");
    void StopClient();
    void StopServer();
}
