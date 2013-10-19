﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DnDCS.Libs.ServerEvents;
using DnDCS.Libs.SimpleObjects;

namespace DnDCS.Libs
{
    public class ServerSocketConnection
    {
        private Thread serverListenerThread;
        private Timer socketPollTimer;

        private readonly int port;
        private Socket server;
        private readonly List<Socket> clients = new List<Socket>();
        public bool IsStopping { get; private set; }

        public event Action OnClientConnected;
        public event Action<ServerEvent> OnSocketEvent;
        public event Action<int> OnClientCountChanged;

        public int ClientsCount { get; private set; }

        public ServerSocketConnection(int port)
        {
            this.port = port;

            serverListenerThread = new Thread(Start);
            serverListenerThread.IsBackground = true;
            serverListenerThread.Name = "Server Socket Thread";
            serverListenerThread.Start();

            socketPollTimer = new Timer(PollTimerCallback, null, ConfigValues.PingInterval, ConfigValues.PingInterval);
        }

        private void PollTimerCallback(object state)
        {
            Write(SimpleObjects.SocketConstants.PingSocketObject, false);
        }

        private void Start()
        {
            try
            {
                CreateServerSocket();
            }
            catch (Exception e)
            {
                Logger.LogError("Server Socket - An error occurred on the server.", e);
                this.Stop();
                return;
            }

            while (!IsStopping)
            {
                try
                {
                    var newClient = TryConnectClient();
                    if (newClient == null)
                        continue;

                    lock (clients)
                    {
                        if (IsStopping)
                            throw new InvalidOperationException("Server Socket - Server has been closed after a new connection was established.");
                        clients.Add(newClient);
                        ClientsCount++;
                        if (OnClientCountChanged != null)
                            OnClientCountChanged(ClientsCount);
                    }
                    if (OnClientConnected != null)
                        OnClientConnected();
                    if (OnSocketEvent != null)
                        OnSocketEvent(new ServerEvent(newClient, ServerEvent.SocketEventType.ClientConnected));
                }
                catch (ThreadInterruptedException e)
                {
                    // Ignore interruptions that may occur.
                }
                catch (Exception e)
                {
                    Logger.LogError("Server Socket - An error occurred on the server.", e);
                }
            }
        }

        private void CreateServerSocket()
        {
            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the host running the application.
            var ipHostInfo = Dns.Resolve(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            var localEndPoint = new IPEndPoint(ipAddress, port);

            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for an incoming connection.
            server.Bind(localEndPoint);
            server.Listen(100);
        }

        private Socket TryConnectClient()
        {
            try
            {
                Logger.LogDebug("Server Socket - Waiting for a connection...");
                var connectedSocket = server.Accept();
                Logger.LogDebug(string.Format("Server Socket - Connection received for '{0}'.", ((IPEndPoint)connectedSocket.RemoteEndPoint).Address));

                var connectedClient = connectedSocket;

                if (WriteAcknowledge(connectedClient))
                    return connectedClient;
            }
            catch (SocketException e1)
            {
                // When stopping, we may raise an error from the blocking Accept() socket call that we want to ignore now.
                if (!IsStopping)
                    Logger.LogError("Server Socket - An error occurred trying to establish initial connection to client.", e1);
            }
            catch (Exception e)
            {
                Logger.LogError("Server Socket - An error occurred trying to establish initial connection to client.", e);
            }
            return null;
        }
        
        private bool WriteAcknowledge(Socket newClient)
        {
            try
            {
                Logger.LogDebug("Server Socket - Writing Acknowledge to new client.");
                var bytes = SocketConstants.AcknowledgeSocketObject.GetBytes();

                var sendBytes = BitConverter.GetBytes(bytes.Length).Concat(bytes).ToArray();
                Logger.LogDebug(string.Format("Server Socket - Writing {0} total bytes.", sendBytes.Length));
                newClient.Send(sendBytes);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError("Server Socket - Failed to write Acknowledge to new client.", e);
                return false;
            }
        }

        public void WriteMap(int mapImageWidth, int mapImageHeight, byte[] mapImageBytes)
        {
            if (ClientsCount == 0)
                return;

            if (mapImageBytes != null && mapImageBytes.Length > 0)
                Write(new ImageSocketObject(SocketConstants.SocketAction.Map, mapImageWidth, mapImageHeight, mapImageBytes));
        }

        public void WriteCenterMap(SimplePoint point)
        {
            if (ClientsCount == 0)
                return;

            Write(new CenterMapSocketObject(point));
        }

        public void WriteFog(int fogImageWidth, int fogImageHeight, byte[] fogImageBytes)
        {
            if (ClientsCount == 0)
                return;

            if (fogImageBytes != null && fogImageBytes.Length > 0)
                Write(new ImageSocketObject(SocketConstants.SocketAction.Fog, fogImageWidth, fogImageHeight, fogImageBytes));
        }

        public void WriteFogUpdate(FogUpdate fogUpdate)
        {
            if (ClientsCount == 0)
                return;

            if (fogUpdate != null && fogUpdate.Length != 0)
                Write(new FogUpdateSocketObject(SocketConstants.SocketAction.FogUpdate, fogUpdate));
        }

        public void WriteGridSize(bool showGrid, int gridSize)
        {
            if (ClientsCount == 0)
                return;

            Write(new GridSizeSocketObject(showGrid, gridSize));
        }

        public void WriteGridColor(SimpleColor color)
        {
            if (ClientsCount == 0)
                return;

            Write(new ColorSocketObject(SocketConstants.SocketAction.GridColor, color.A, color.R, color.G, color.B));
        }

        public void WriteBlackout(bool isBlackoutOn)
        {
            if (ClientsCount == 0)
                return;

            Write(new BaseSocketObject((isBlackoutOn) ? SocketConstants.SocketAction.BlackoutOn : SocketConstants.SocketAction.BlackoutOff));
        }

        private void LogSocketObject(BaseSocketObject socketObject, string message)
        {
            if (socketObject.Action != SocketConstants.SocketAction.Ping || ConfigValues.LogPings)
                Logger.LogDebug(message);
        }

        private void Write(BaseSocketObject socketObject, bool raiseSocketEvent = true)
        {
            if (ClientsCount == 0)
                return;

            try
            {
                LogSocketObject(socketObject, string.Format("Server Socket - Writing Socket Object '{0}'.", socketObject));
                var bytes = socketObject.GetBytes();

                var sendBytes = BitConverter.GetBytes(bytes.Length).Concat(bytes).ToArray();
                LogSocketObject(socketObject, string.Format("Server Socket - Writing {0} total bytes.", sendBytes.Length));
                lock (clients)
                {
                    for (int c = 0; c < clients.Count; c++)
                    {
                        var client = clients[c];
                        try
                        {
                            LogSocketObject(socketObject, string.Format("Server Socket - Writing to '{0}'.", ((IPEndPoint)client.RemoteEndPoint).Address));
                            client.Send(sendBytes);
                            if (raiseSocketEvent && OnSocketEvent != null)
                                OnSocketEvent(new ServerEvent(client, ServerEvent.SocketEventType.DataSent, socketObject.Action.ToString()));
                        }
                        catch (Exception e)
                        {
                            if (e is SocketException && ((SocketException)e).SocketErrorCode != SocketError.ConnectionAborted)
                            {
                                Logger.LogError(string.Format("Server Socket - Client socket '{0}' has been closed already. Fully disconnecting client now.", ((IPEndPoint)client.RemoteEndPoint).Address), e);
                            }
                            else
                            {
                                Logger.LogError(string.Format("Server Socket - Failed to write Socket Object '{0}' to Client '{1}'. Disconnecting client.", socketObject, ((IPEndPoint)client.RemoteEndPoint).Address), e);
                            }

                            // Remove the element from the list and repeat the index, since the subsequent items would shift down.
                            SafeCloseClient(client);
                            clients.RemoveAt(c);
                            ClientsCount--;

                            if (OnClientCountChanged != null)
                                OnClientCountChanged(ClientsCount);

                            c--;
                        }
                    }
                }
                LogSocketObject(socketObject, string.Format("Server Socket - Done writing Socket Object '{0}'.", socketObject));
            }
            catch (Exception e)
            {
                Logger.LogError(string.Format("Server Socket - Failed to write Socket Object '{0}'.", socketObject), e);
            }
        }

        private void SafeCloseClient(Socket client)
        {
            try
            {
                var address = ((IPEndPoint)client.RemoteEndPoint).Address.ToString();

                client.Shutdown(SocketShutdown.Both);
                client.Close();

                if (OnSocketEvent != null)
                    OnSocketEvent(new ServerEvent(address, ServerEvent.SocketEventType.ClientDisconnected));
            }
            catch
            {
            }
        }

        public void Stop()
        {
            IsStopping = true;
            Logger.LogDebug("Server Socket - Stopping...");

            if (ClientsCount > 0)
            {
                Logger.LogDebug("Server Socket - Sending 'Exit' to clients...");
                Write(SocketConstants.ExitSocketObject);
                Logger.LogDebug("Server Socket - 'Exit' Sent to clients.");

                Logger.LogDebug("Server Socket - Closing Client sockets...");
                lock (clients)
                {
                    clients.ForEach(client =>
                        {
                            SafeCloseClient(client);
                            ClientsCount--;
                            
                            if (OnClientCountChanged != null)
                                OnClientCountChanged(ClientsCount);
                        });
                    clients.Clear();
                }
                Logger.LogDebug("Server Socket - Client Sockets closed.");
            }

            if (server != null)
            {
                Logger.LogDebug("Server Socket - Closing Server sockets...");
                server.Close();
                server = null;
                Logger.LogDebug("Server Socket - Server Sockets closed.");
            }

            Logger.LogDebug("Server Socket - Stopped.");
        }
    }
}
