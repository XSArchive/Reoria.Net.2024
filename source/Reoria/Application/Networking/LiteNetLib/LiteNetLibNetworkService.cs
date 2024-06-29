using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Reoria.Application.Networking.Enumerations;
using Reoria.Application.Networking.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace Reoria.Application.Networking.LiteNetLib;

public class LiteNetLibNetworkService : INetworkService
{
    protected readonly ILogger<LiteNetLibNetworkService> logger;
    protected readonly EventBasedNetListener listener;
    protected readonly NetManager manager;
    protected int maxConnections;
    protected string connectionKey;
    protected NetworkMode networkMode;

    public LiteNetLibNetworkService(ILogger<LiteNetLibNetworkService> logger)
    {
        this.logger = logger;
        this.listener = new();
        this.manager = new(this.listener);
        this.connectionKey = string.Empty;
        this.networkMode = NetworkMode.None;

        this.listener.ConnectionRequestEvent += this.OnConnectionRequestEvent;
        this.listener.NetworkReceiveEvent += this.OnNetworkReceiveEvent;
        this.listener.NetworkErrorEvent += this.OnNetworkError;
        this.listener.PeerConnectedEvent += this.OnPeerConnectedEvent;
        this.listener.PeerDisconnectedEvent += this.OnPeerDisconnected;
    }

    public void PollEvents() => this.manager.PollEvents();

    public void StartClient(string ipAddress, int port, string connectionKey)
    {
        if (!this.manager.IsRunning)
        {
            this.logger.LogInformation("Starting network client...");

            if (this.manager.Start())
            {
                this.logger.LogInformation("Connecting network client to '{ipAddress}:{port}'...", ipAddress, port);
                _ = this.manager.Connect(ipAddress, port, connectionKey);
                this.networkMode = NetworkMode.Client;
            }
        }
    }

    public void StartServer(int port, int maxConnections, string connectionKey)
    {
        if (!this.manager.IsRunning)
        {
            this.logger.LogInformation("Starting network server...");

            if (this.manager.Start(port))
            {
                this.logger.LogInformation("Network server is listening on port {port}, maximum connections {maxConnections}...", port, maxConnections);
                this.maxConnections = maxConnections;
                this.connectionKey = connectionKey;
                this.networkMode = NetworkMode.Server;
            }
        }
    }

    public void Stop()
    {
        if (this.manager.IsRunning)
        {
            this.logger.LogInformation("Stopping network {type}...", this.networkMode.ToString().ToLower());
            this.manager.Stop();
            this.networkMode = NetworkMode.None;
        }
    }

    public void SendTo(NetDataWriter writer, DeliveryMethod deliveryMethod)
    {
        if (this.networkMode == NetworkMode.Client)
        {
            NetPeer serverPeer = this.manager.FirstPeer;

            if (serverPeer != null)
            {
                serverPeer.Send(writer, deliveryMethod);
            }
            else
            {
                this.logger.LogWarning("No server connection available to send data.");
            }
        }
        else
        {
            this.logger.LogWarning("SendTo without a peer can only be called in client mode.");
        }
    }

    public void SendTo(NetPeer peer, NetDataWriter writer, DeliveryMethod deliveryMethod)
    {
        if (this.networkMode == NetworkMode.Server)
        {
            peer.Send(writer, deliveryMethod);
        }
        else
        {
            this.logger.LogWarning("SendTo with a peer can only be called in server mode.");
        }
    }

    public void SendToAllBut(NetPeer excludedPeer, NetDataWriter writer, DeliveryMethod deliveryMethod)
    {
        if (this.networkMode == NetworkMode.Server)
        {
            foreach (NetPeer peer in this.manager.ConnectedPeerList)
            {
                if (peer != excludedPeer)
                {
                    peer.Send(writer, deliveryMethod);
                }
            }
        }
        else
        {
            this.logger.LogWarning("SendToAllBut can only be called in server mode.");
        }
    }

    public void SendToAll(NetDataWriter writer, DeliveryMethod deliveryMethod)
    {
        if(this.networkMode == NetworkMode.Server)
        {
            foreach (NetPeer peer in this.manager.ConnectedPeerList)
            {
                peer.Send(writer, deliveryMethod);
            }
        }
        else
        {
            this.logger.LogWarning("SendToAll can only be called in server mode.");
        }
    }

    private void OnConnectionRequestEvent(ConnectionRequest request)
    {
        if(this.networkMode == NetworkMode.Server)
        {
            if (this.manager.ConnectedPeersCount < this.maxConnections)
            {
                string connectionKey = request.Data.PeekString();

                if (connectionKey.Equals(this.connectionKey))
                {
                    this.logger.LogInformation("Server accepted connection from {peer}.", request.RemoteEndPoint);
                    _ = request.Accept();
                }
                else
                {
                    this.logger.LogWarning("Server rejected connection from {peer}, Reason: Invalid connection key '{key}'.", request.RemoteEndPoint, connectionKey);
                    request.Reject();
                }                
            }
            else
            {
                this.logger.LogWarning("Server rejected connection from {peer}, Reason: Server is full.", request.RemoteEndPoint);
                request.Reject();
            }
        }
        else
        {
            this.logger.LogWarning("Client rejected connection from {peer}, Reason: Client can not accept connections.", request.RemoteEndPoint);
            request.Reject();
        }
    }

    private void OnPeerConnectedEvent(NetPeer peer)
    {
        if(this.networkMode == NetworkMode.Server)
        {
            this.logger.LogInformation("Server got connection: {peer} ({number} of {maxConnections})", peer, this.manager.ConnectedPeersCount, this.maxConnections);
            NetDataWriter writer = new();
            writer.Put($"Hello {peer}!");
            this.SendTo(peer, writer, DeliveryMethod.ReliableOrdered);
            
            writer.Reset();
            writer.Put($"{peer} has joined the server!");
            this.SendToAllBut(peer, writer, DeliveryMethod.ReliableOrdered);
        }
        else if (this.networkMode == NetworkMode.Client)
        {
            this.logger.LogInformation("Client got connection to server: {peer}", peer);
            NetDataWriter writer = new();
            writer.Put($"Hello server {peer}!");
            this.SendTo(writer, DeliveryMethod.ReliableOrdered);
        }
    }

    private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if(this.networkMode == NetworkMode.Server)
        {
            this.logger.LogInformation("Server lost connection with {peer}, Reason: {disconnectInfo}", peer, disconnectInfo.Reason);

            NetDataWriter writer = new();
            writer.Put($"{peer} has left the server!");
            this.SendToAllBut(peer, writer, DeliveryMethod.ReliableOrdered);
        }
        else if(this.networkMode == NetworkMode.Client)
        {
            this.logger.LogInformation("Client disconnected from server {peer}, Reason: {disconnectInfo}", peer, disconnectInfo.Reason);
            this.Stop();
        }
    }

    private void OnNetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        if (this.networkMode == NetworkMode.Server)
        {
            this.logger.LogInformation("Server got data from {peer}: {packet}", peer, reader.GetString());
            reader.Recycle();
        }
        else if (this.networkMode == NetworkMode.Client)
        {
            this.logger.LogInformation("Client got data: {packet}", reader.GetString());
            reader.Recycle();
        }
    }

    private void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        this.logger.LogError("Network error on {endPoint}, Reason: {socketError}", endPoint, socketError);

        if (this.networkMode == NetworkMode.Server)
        {
            NetDataWriter writer = new();
            writer.Put($"{endPoint} has left the server!");
            this.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        }
        else if (this.networkMode == NetworkMode.Client)
        {
            this.Stop();
        }
    }
}