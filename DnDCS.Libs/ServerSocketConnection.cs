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
        private readonly int port;
        private bool isConnected;
        private Socket server;
        private Socket client;

        public ServerSocketConnection(int port)
        {
            this.port = port;

            var socketThread = new Thread(Start);
            socketThread.Name = "Server Socket Thread";
            socketThread.Start();
        }

        private void Start()
        {
            try
            {
                TryConnect();
                if (client == null)
                {
                    this.Stop();
                    return;
                }

                isConnected = true;
            }
            catch (Exception e)
            {
                Logger.LogError("Server Socket - An error occurred on the server.", e);
                this.Stop();
            }
        }

        private void TryConnect()
        {
            try
            {
                // Establish the local endpoint for the socket.
                // Dns.GetHostName returns the name of the host running the application.
                var ipHostInfo = Dns.Resolve(Dns.GetHostName());
                var ipAddress = ipHostInfo.AddressList[0];
                var localEndPoint = new IPEndPoint(ipAddress, port);

                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Bind the socket to the local endpoint and listen for an incoming connection.
                server.Bind(localEndPoint);
                server.Listen(10);

                Logger.LogDebug("Server Socket - Waiting for a connection...");
                var connectedSocket = server.Accept();
                Logger.LogDebug(string.Format("Server Socket - Connection received for '{0}'.", ((IPEndPoint)connectedSocket.RemoteEndPoint).Address));
                
                client = connectedSocket;

                Write(SocketConstants.AcknowledgeSocketObject);
            }
            catch (Exception e)
            {
                Logger.LogError("Server Socket - An error occurred trying to establish initial connection to client.", e);
            }
        }
        
        public void WriteMap(Image map)
        {
            Write(ImageSocketObject.CreateMap(map));
        }

        public void WriteFog(Image fog)
        {
            Write(ImageSocketObject.CreateFog(fog));
        }

        public void WriteFogUpdate(Point[] fogUpdate, bool isClearing)
        {
            Write(new PointArraySocketObject(SocketConstants.SocketAction.FogUpdate, fogUpdate, isClearing));
        }

        public void WriteBlackout(bool isBlackoutOn)
        {
            Write(new BaseSocketObject((isBlackoutOn) ? SocketConstants.SocketAction.BlackoutOn : SocketConstants.SocketAction.BlackoutOff));
        }

        private void Write(BaseSocketObject socketObject)
        {
            if (client == null)
                throw new ObjectDisposedException("Underlying socket has already been closed.");

            try
            {
                Logger.LogDebug(string.Format("Server Socket - Writing Socket Object '{0}'.", socketObject));
                var bytes = socketObject.GetBytes();
                bytes.Add((byte)SocketConstants.SocketAction.EndOfData);
                client.Send(bytes.ToArray());
            }
            catch (Exception e)
            {
                Logger.LogError(string.Format("Server Socket - Failed to write Socket Object '{0}'.", socketObject), e);
            }
        }

        public void Stop()
        {
            Logger.LogDebug("Server Socket - Stopping...");

            if (client != null)
            {
                if (isConnected)
                {
                    Logger.LogDebug("Server Socket - Sending 'Exit' to client...");
                    Write(SocketConstants.ExitSocketObject);
                    Logger.LogDebug("Server Socket - 'Exit' Sent to client.");
                }
                Logger.LogDebug("Server Socket - Closing Client socket...");
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                client = null;
                Logger.LogDebug("Server Socket - Client Socket closed.");
            }

            if (server != null)
            {
                Logger.LogDebug("Server Socket - Closing Server socket...");
                server.Close();
                server = null;
                Logger.LogDebug("Server Socket - Server Socket closed.");
            }

            Logger.LogDebug("Server Socket - Stopped.");
        }
    }
}
