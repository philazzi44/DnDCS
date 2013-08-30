using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Drawing;
using DnDCS.Libs.SocketObjects;
using System.IO;

namespace DnDCS.Libs
{
    public class ServerSocketConnection
    {
        private Thread socketThread;

        private readonly int port;
        private Socket server;
        private readonly List<Socket> clients = new List<Socket>();
        private bool isStopping;

        public event Action OnClientConnected;

        public ServerSocketConnection(int port)
        {
            this.port = port;

            socketThread = new Thread(Start);
            socketThread.Name = "Server Socket Thread";
            socketThread.Start();
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

            while (!isStopping)
            {
                try
                {
                    var newClient = TryConnectClient();
                    if (newClient == null)
                        continue;

                    lock (newClient)
                    {
                        clients.Add(newClient);
                    }
                    if (OnClientConnected != null)
                        OnClientConnected();
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

        public void WriteMap(Image map)
        {
            if (map != null)
                Write(ImageSocketObject.CreateMap(map));
        }

        public void WriteFog(Image fog)
        {
            if (fog != null)
                Write(ImageSocketObject.CreateFog(fog));
        }

        public void WriteFogUpdate(Point[] fogUpdate, bool isClearing)
        {
            if (fogUpdate != null && fogUpdate.Length != 0)
                Write(new PointArraySocketObject(SocketConstants.SocketAction.FogUpdate, fogUpdate, isClearing));
        }

        public void WriteBlackout(bool isBlackoutOn)
        {
            Write(new BaseSocketObject((isBlackoutOn) ? SocketConstants.SocketAction.BlackoutOn : SocketConstants.SocketAction.BlackoutOff));
        }

        private void Write(BaseSocketObject socketObject)
        {
            if (!clients.Any())
                return;

            try
            {
                Logger.LogDebug(string.Format("Server Socket - Writing Socket Object '{0}'.", socketObject));
                var bytes = socketObject.GetBytes();

                var sendBytes = BitConverter.GetBytes(bytes.Length).Concat(bytes).ToArray();
                Logger.LogDebug(string.Format("Server Socket - Writing {0} total bytes.", sendBytes.Length));
                for (int c = 0; c < clients.Count; c++)
                {
                    var client = clients[c];
                    try
                    {
                        Logger.LogDebug(string.Format("Server Socket - Writing to '{0}'.", ((IPEndPoint)client.RemoteEndPoint).Address));
                        client.Send(sendBytes);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(string.Format("Server Socket - Failed to write Socket Object '{0}' to Client '{1}'. Disconnecting client.", socketObject, ((IPEndPoint)client.RemoteEndPoint).Address), e);
                        // Remove the element from the list and repeat the index, since the subsequent items would shift down.
                        SafeClose(client);
                        clients.RemoveAt(c);
                        c--;
                    }
                }
                Logger.LogDebug(string.Format("Server Socket - Done writing Socket Object '{0}'.", socketObject));
            }
            catch (Exception e)
            {
                Logger.LogError(string.Format("Server Socket - Failed to write Socket Object '{0}'.", socketObject), e);
            }
        }

        private void SafeClose(Socket socket)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch
            {
            }
        }

        public void Stop()
        {
            isStopping = true;
            Logger.LogDebug("Server Socket - Stopping...");

            if (clients.Any())
            {
                Logger.LogDebug("Server Socket - Sending 'Exit' to clients...");
                Write(SocketConstants.ExitSocketObject);
                Logger.LogDebug("Server Socket - 'Exit' Sent to clients.");

                Logger.LogDebug("Server Socket - Closing Client sockets...");
                clients.ForEach(client =>
                    {
                        SafeClose(client);
                    });
                clients.Clear();
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
