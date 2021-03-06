﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DnDCS.Libs.ServerEvents;
using DnDCS.Libs.SimpleObjects;
using DnDCS.Libs.ClientSockets;
using SuperWebSocket;
using SuperSocket.SocketBase;

namespace DnDCS.Libs
{
    public class ServerSocketConnection
    {
        private readonly Thread serverNetSocketListenerThread;
        private Timer socketPollTimer;

        private Socket netSocketServer;
        private WebSocketServer webSocketServer;
        private readonly List<ClientSocket> clients = new List<ClientSocket>();
        public bool IsStopping { get; private set; }

        public event Action OnClientConnected;
        public event Action<ServerEvent> OnSocketEvent;
        public event Action<int> OnClientCountChanged;

        public int ClientsCount { get { return NetClientsCount + WebClientsCount; } }
        public int NetClientsCount { get; private set; }
        public int WebClientsCount { get; private set; }

        public IPAddress ServerIPAddress
        {
            get
            {
                // Dns.GetHostName returns the name of the host running the application.
                var ipHostInfo = Dns.Resolve(Dns.GetHostName());
                return ipHostInfo.AddressList[0];
            }
        }

        public string ServerIP { get { return ServerIPAddress.ToString(); } }
        public int NetSocketPort { get; private set; }
        public int WebSocketPort { get; private set; }

        private Queue<BaseSocketObject> writeQueue = new Queue<BaseSocketObject>();
        private readonly AutoResetEvent writeQueueEvent = new AutoResetEvent(false);
        private readonly Thread writeQueueThread;

        public ServerSocketConnection(int netSocketPort, int webSocketPort)
        {
            this.NetSocketPort = netSocketPort;
            this.WebSocketPort = webSocketPort;

            this.serverNetSocketListenerThread = new Thread(NetStart)
                                                    {
                                                        IsBackground = true,
                                                        Name = Constants.ServerNetSocketString
                                                    };

            this.writeQueueThread = new Thread(WriteStart)
            {
                IsBackground = true,
                Name = Constants.ServerWriteQueueThreadName
            };

            ThreadPool.QueueUserWorkItem(new WaitCallback(state =>
            {
                try
                {
                    CreateServerNetSocket();
                    CreateServerWebSocket();
                }
                catch (Exception e)
                {
                    Logger.LogError(CreateLogMessage(Constants.ServerSocketsString, "An error occurred on the server."), e);
                    this.Stop();
                    return;
                }

                this.serverNetSocketListenerThread.Start();
                this.webSocketServer.Start();
                this.writeQueueThread.Start();

                socketPollTimer = new Timer(PollTimerCallback, null, ConfigValues.PingInterval, ConfigValues.PingInterval);
            }));
        }

        #region Net Logic

        private void CreateServerNetSocket()
        {
            // Establish the local endpoint for the socket.
            var localEndPoint = new IPEndPoint(this.ServerIPAddress, this.NetSocketPort);

            netSocketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for an incoming connection.
            netSocketServer.Bind(localEndPoint);
            netSocketServer.Listen(100);
        }

        private void NetStart()
        {
            while (!IsStopping)
            {
                try
                {
                    var newClient = TryConnectNetClient();
                    if (newClient == null)
                        continue;

                    lock (clients)
                    {
                        if (IsStopping)
                            throw new InvalidOperationException(CreateLogMessage(Constants.ServerNetSocketString, "Server has been closed after a new connection was established."));
                        clients.Add(newClient);
                        NetClientsCount++;
                        if (OnClientCountChanged != null)
                            OnClientCountChanged(ClientsCount);
                    }
                    if (OnClientConnected != null)
                        OnClientConnected();
                    if (OnSocketEvent != null)
                        OnSocketEvent(new ServerEvent(newClient, ServerEvent.SocketEventType.NetClientConnected));
                }
                catch (ThreadInterruptedException e)
                {
                    // Ignore interruptions that may occur.
                }
                catch (Exception e)
                {
                    Logger.LogError(CreateLogMessage(Constants.ServerNetSocketString, "An error occurred on the server."), e);
                }
            }
        }

        private ClientNetSocket TryConnectNetClient()
        {
            try
            {
                Logger.LogDebug(CreateLogMessage(Constants.ServerNetSocketString, "Waiting for a connection..."));

                var connectedSocket = netSocketServer.Accept();
                
                if (IsStopping)
                    throw new InvalidOperationException(CreateLogMessage(Constants.ServerNetSocketString, "Server has been closed after a new connection was established."));

                var connectedClientNetSocket = new ClientNetSocket(connectedSocket);

                Logger.LogDebug(CreateLogMessage(Constants.ServerNetSocketString, string.Format("Connection received for '{0}'.", connectedClientNetSocket.Address)));

                if (WriteAcknowledge(connectedClientNetSocket))
                    return connectedClientNetSocket;
            }
            catch (SocketException e1)
            {
                // When stopping, we may raise an error from the blocking Accept() socket call that we want to ignore now.
                if (!IsStopping)
                    Logger.LogError(CreateLogMessage(Constants.ServerNetSocketString, "An error occurred trying to establish initial connection to client."), e1);
            }
            catch (Exception e)
            {
                Logger.LogError(CreateLogMessage(Constants.ServerNetSocketString, "An error occurred trying to establish initial connection to client."), e);
            }
            return null;
        }

        #endregion Net Logic

        #region Web Logic
        
        private void CreateServerWebSocket()
        {
            var rootConfig = new SuperSocket.SocketBase.Config.RootConfig();
            var serverConfig = new SuperSocket.SocketBase.Config.ServerConfig
            {
                Name = Constants.ServerWebSocketString,
                Ip = "Any",
                Port = this.WebSocketPort,
                Mode = SocketMode.Tcp
            };

            this.webSocketServer = new WebSocketServer();
            this.webSocketServer.Setup(rootConfig, serverConfig, new SuperSocket.SocketEngine.SocketServerFactory());
            this.webSocketServer.NewSessionConnected += new SessionHandler<WebSocketSession>(ServerWebSocket_NewSessionConnected);
            this.webSocketServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(ServerWebSocket_NewMessageReceived);
            this.webSocketServer.NewDataReceived += new SessionHandler<WebSocketSession, byte[]>(ServerWebSocket_NewDataReceived);
        }

        private void ServerWebSocket_NewSessionConnected(WebSocketSession session)
        {
            var newClient = new ClientWebSocket(session);

            try
            {
                Logger.LogDebug(CreateLogMessage(Constants.ServerWebSocketString, string.Format("Connection received for '{0}'.", newClient.Address)));

                if (IsStopping)
                {
                    Logger.LogError(CreateLogMessage(Constants.ServerWebSocketString, "Server has been closed after a new connection was established."));
                    SafeCloseClient(newClient);
                    return;
                }

                if (!WriteAcknowledge(newClient))
                {
                    Logger.LogError(CreateLogMessage(Constants.ServerWebSocketString, "An error occurred trying to establish initial connection to client."));
                    SafeCloseClient(newClient);
                    return;
                }

                lock (clients)
                {
                    clients.Add(newClient);
                    WebClientsCount++;
                    if (OnClientCountChanged != null)
                        OnClientCountChanged(ClientsCount);
                }
                if (OnClientConnected != null)
                    OnClientConnected();
                if (OnSocketEvent != null)
                    OnSocketEvent(new ServerEvent(newClient, ServerEvent.SocketEventType.WebClientConnected));
            }
            catch (Exception e)
            {
                if (!IsStopping)
                    Logger.LogError(CreateLogMessage(Constants.ServerWebSocketString, "An error occurred on the server."), e);
                SafeCloseClient(newClient);
            }
        }

        private void ServerWebSocket_NewMessageReceived(WebSocketSession session, string message)
        {
            Logger.LogError("I don't know what this does yet, but the string received is: " + message);
            throw new InvalidOperationException();
        }

        private void ServerWebSocket_NewDataReceived(WebSocketSession session, byte[] data)
        {
            Logger.LogError("I don't know what this does yet, but the data received became this as UTF8 string: " + System.Text.Encoding.UTF8.GetString(data));
            throw new InvalidOperationException();
        }

        #endregion Web Logic

        #region Write Logic

        private bool WriteAcknowledge(ClientSocket newClient)
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
                EnqueueWrite(new ImageSocketObject(SocketConstants.SocketAction.Map, mapImageWidth, mapImageHeight, mapImageBytes));
        }

        public void WriteCenterMap(SimplePoint point)
        {
            if (ClientsCount == 0)
                return;

            EnqueueWrite(new CenterMapSocketObject(point));
        }

        public void WriteFog(int fogImageWidth, int fogImageHeight, byte[] fogImageBytes)
        {
            if (ClientsCount == 0)
                return;

            if (fogImageBytes != null && fogImageBytes.Length > 0)
                EnqueueWrite(new ImageSocketObject(SocketConstants.SocketAction.Fog, fogImageWidth, fogImageHeight, fogImageBytes));
        }

        public void WriteFogUpdate(FogUpdate fogUpdate)
        {
            if (ClientsCount == 0)
                return;

            if (fogUpdate != null && fogUpdate.Length != 0)
                EnqueueWrite(new FogUpdateSocketObject(SocketConstants.SocketAction.FogUpdate, fogUpdate));
        }

        public void WriteFogOrRevealAll(bool fogAll)
        {
            if (ClientsCount == 0)
                return;

            EnqueueWrite(new FogOrRevealAllSocketObject(SocketConstants.SocketAction.FogOrRevealAll, fogAll));
        }

        public void WriteUseFogAlphaEffect(bool useFogAlphaEffect)
        {
            if (ClientsCount == 0)
                return;

            EnqueueWrite(new UseFogAlphaEffectSocketObject(useFogAlphaEffect));
        }

        public void WriteGridSize(bool showGrid, int gridSize)
        {
            if (ClientsCount == 0)
                return;

            EnqueueWrite(new GridSizeSocketObject(showGrid, gridSize));
        }

        public void WriteGridColor(SimpleColor color)
        {
            if (ClientsCount == 0)
                return;

            EnqueueWrite(new ColorSocketObject(SocketConstants.SocketAction.GridColor, color.A, color.R, color.G, color.B));
        }

        public void WriteBlackout(bool isBlackoutOn)
        {
            if (ClientsCount == 0)
                return;

            EnqueueWrite(new BaseSocketObject((isBlackoutOn) ? SocketConstants.SocketAction.BlackoutOn : SocketConstants.SocketAction.BlackoutOff));
        }
        
        /// <summary> Adds the Socket Object to the Write Queue and notifies the Write thread. </summary>
        private void EnqueueWrite(BaseSocketObject socketObject)
        {
            lock (this.writeQueue)
            {
                this.writeQueue.Enqueue(socketObject);
                this.writeQueueEvent.Set();
            }
        }

        /// <summary> Threaded callback that will churn through the Write Queue and send the items out. </summary>
        private void WriteStart()
        {
                while (!this.IsStopping)
                {
                    try
                    {
                        // Safely dequeue an item, if any exists
                        BaseSocketObject socketObject = null;
                        lock (this.writeQueue)
                        {
                            if (this.writeQueue.Any())
                            {
                                socketObject = this.writeQueue.Dequeue();
                            }
                        }

                        // If an item was dequeued, send it.
                        // Otherwise, wait until one is enqueue.
                        // If one was enqueue after we checked if one was available to dequeue, then the event will already be Set so the WaitOne will return immediately.
                        if (socketObject != null)
                            Write(socketObject);
                        else
                            this.writeQueueEvent.WaitOne();
                    }
                    catch
                    {
                    }
                }
        }

        /// <summary> Writes the Socket Object to the clients, if any are connected. Consumes all exceptions. </summary>
        private void Write(BaseSocketObject socketObject)
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
                    for (var c = 0; c < clients.Count; c++)
                    {
                        var client = clients[c];
                        try
                        {
                            LogSocketObject(socketObject, string.Format("Server Socket - Writing to '{0}'.", client.Address));
                            client.Send(sendBytes);
                            if (OnSocketEvent != null)
                            {
                                switch (socketObject.Action)
                                {
                                    // Pings never raise up the event, otherwise we'd be flooded with events.
                                    case SocketConstants.SocketAction.Ping:
                                        break;
                                    default:
                                        OnSocketEvent(new ServerEvent(client, socketObject.Action));
                                        break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (e is SocketException && ((SocketException)e).SocketErrorCode != SocketError.ConnectionAborted)
                            {
                                Logger.LogError(string.Format("Server Socket - Client socket '{0}' has been closed already. Fully disconnecting client now.", client.Address), e);
                            }
                            else
                            {
                                Logger.LogError(string.Format("Server Socket - Failed to write Socket Object '{0}' to Client '{1}'. Disconnecting client.", socketObject, client.Address), e);
                            }

                            // Remove the element from the list and repeat the index, since the subsequent items would shift down.
                            SafeCloseClient(client);
                            clients.RemoveAt(c);
                            if (client is ClientNetSocket)
                                NetClientsCount--;
                            else if (client is ClientWebSocket)
                                WebClientsCount--;

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

        #endregion Write Logic

        private void PollTimerCallback(object state)
        {
            EnqueueWrite(SimpleObjects.SocketConstants.PingSocketObject);
        }

        private void LogSocketObject(BaseSocketObject socketObject, string message)
        {
            if (socketObject.Action != SocketConstants.SocketAction.Ping || ConfigValues.LogPings)
                Logger.LogDebug(message);
        }

        private void SafeCloseClient(ClientSocket client)
        {
            try
            {
                var address = client.Address;

                client.Dispose();

                if (OnSocketEvent != null)
                    OnSocketEvent(new ServerEvent(address, (client is ClientNetSocket) ? ServerEvent.SocketEventType.NetClientDisconnected : ServerEvent.SocketEventType.WebClientDisconnected));
            }
            catch
            {
            }
        }

        public void Stop()
        {
            IsStopping = true;
            Logger.LogDebug("Server Sockets - Stopping...");

            if (ClientsCount > 0)
            {
                Logger.LogDebug("Server Sockets - Sending 'Exit' to clients...");
                Write(SocketConstants.ExitSocketObject);
                Logger.LogDebug("Server Sockets - 'Exit' Sent to clients.");

                Logger.LogDebug("Server Sockets - Closing Client sockets...");
                lock (clients)
                {
                    clients.ForEach(client =>
                        {
                            SafeCloseClient(client);
                            if (client is ClientNetSocket)
                                NetClientsCount--;
                            else if (client is ClientWebSocket)
                                WebClientsCount--;
                            
                            if (OnClientCountChanged != null)
                                OnClientCountChanged(ClientsCount);
                        });
                    clients.Clear();
                }
                Logger.LogDebug("Server Sockets - Client Sockets closed.");
            }

            if (netSocketServer != null)
            {
                Logger.LogDebug("Server Net Socket - Closing Server socket...");
                netSocketServer.Close();
                netSocketServer = null;
                Logger.LogDebug("Server Net Socket - Server Socket closed.");
            }

            if (webSocketServer != null)
            {
                Logger.LogDebug("Server Web Socket - Closing Server socket...");
                webSocketServer.Stop();
                webSocketServer.Dispose();
                webSocketServer = null;
                Logger.LogDebug("Server Web Socket - Server Socket closed.");
            }

            if (socketPollTimer != null)
                socketPollTimer.Dispose();

            if (this.writeQueueEvent != null)
            {
                this.writeQueueEvent.Set();
                writeQueueEvent.Dispose();
            }

            Logger.LogDebug("Server Sockets - Stopped.");
        }

        private static string CreateLogMessage(string name, string message)
        {
            return string.Format("{0} - {1}", name, message);
        }
    }
}
