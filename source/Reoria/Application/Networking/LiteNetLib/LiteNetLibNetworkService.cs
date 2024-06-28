using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Reoria.Application.Networking.Interfaces;

namespace Reoria.Application.Networking.LiteNetLib;

public class LiteNetLibNetworkService : INetworkService
{
    protected readonly ILogger<LiteNetLibNetworkService> logger;
    protected readonly EventBasedNetListener listener;
    protected readonly NetManager manager;
    protected int maxConnections;
    protected string connectionKey;

    public LiteNetLibNetworkService(ILogger<LiteNetLibNetworkService> logger)
    {
        this.logger = logger;
        this.listener = new();
        this.manager = new(this.listener);
        this.connectionKey = string.Empty;
    }

    public void PollEvents() => this.manager.PollEvents();

    public void StartClient(string ipAddress = "localhost", int port = 9050, string connectionKey = "SomeConnectionKey")
    {
        if (!this.manager.IsRunning)
        {
            this.logger.LogInformation("Starting network client...");
            if (this.manager.Start())
            {
                this.logger.LogInformation("Connecting network client to '{ipAddress}:{port}'...", ipAddress, port);
                _ = this.manager.Connect(ipAddress, port, connectionKey);
                this.listener.NetworkReceiveEvent += this.Listener_Client_NetworkReceiveEvent;
            }
        }
    }

    public void StopClient()
    {
        if (this.manager.IsRunning)
        {
            this.logger.LogInformation("Stopping network client...");
            this.manager.Stop();
            this.listener.NetworkReceiveEvent -= this.Listener_Client_NetworkReceiveEvent;
        }
    }

    private void Listener_Client_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        this.logger.LogInformation("Client got data: {packet}", reader.GetString());
        reader.Recycle();
    }

    public void StartServer(int port = 9050, int maxConnections = 10, string connectionKey = "SomeConnectionKey")
    {
        if (!this.manager.IsRunning)
        {
            this.logger.LogInformation("Starting network server...");
            if (this.manager.Start(port))
            {
                this.logger.LogInformation("Network server is listening on port {port}, maximum connections {maxConnections}...", port, maxConnections);
                this.maxConnections = maxConnections;
                this.connectionKey = connectionKey;
                this.listener.ConnectionRequestEvent += this.Listener_Server_ConnectionRequestEvent;
                this.listener.PeerConnectedEvent += this.Listener_Server_PeerConnectedEvent;
                this.listener.PeerDisconnectedEvent += this.Listener_Server_PeerDisconnectedEvent;
            }
        }
    }

    public void StopServer()
    {
        if (this.manager.IsRunning)
        {
            this.logger.LogInformation("Stopping network server...");
            this.manager.Stop();
            this.listener.ConnectionRequestEvent -= this.Listener_Server_ConnectionRequestEvent;
            this.listener.PeerConnectedEvent -= this.Listener_Server_PeerConnectedEvent;
        }
    }

    private void Listener_Server_ConnectionRequestEvent(ConnectionRequest request)
    {
        if (this.manager.ConnectedPeersCount < this.maxConnections)
            _ = request.AcceptIfKey(this.connectionKey);
        else
            request.Reject();
    }

    private void Listener_Server_PeerConnectedEvent(NetPeer peer)
    {
        this.logger.LogInformation("Server got connection: {peer} ({number} of {maxConnections})", peer, this.manager.ConnectedPeersCount, this.maxConnections);
        NetDataWriter writer = new();
        writer.Put($"Hello {peer}!");
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private void Listener_Server_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        this.logger.LogInformation("Server lost connection: {peer}", peer);
        this.logger.LogInformation("    -> Reason: {disconnectInfo}", disconnectInfo.Reason);
    }
}