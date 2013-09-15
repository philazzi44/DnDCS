using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Drawing;
using DnDCS.Libs.SocketObjects;

namespace DnDCS.Libs
{
    public class ClientSocketConnection
    {
        private Socket server;

        public string Address { get { return address; } }
        public int Port { get { return port; } }
        private readonly string address;
        private readonly int port;
        private bool isStopped;

        public event Action<Image> OnMapReceived;
        public event Action<Image> OnFogReceived;
        public event Action<Point[], bool> OnFogUpdateReceived;
        public event Action<bool, int> OnGridSizeReceived;
        public event Action<Color> OnGridColorReceived;
        public event Action OnExitReceived;
        public event Action<bool> OnBlackoutReceived;

        public ClientSocketConnection(string address, int port)
        {
            this.address = address;
            this.port = port;

            var socketThread = new Thread(Start);
            socketThread.IsBackground = true;
            socketThread.Name = "Client Socket Thread";
            socketThread.Start();
        }

        private void Start()
        {
            if (isStopped)
                throw new ObjectDisposedException("Client Connection - Cannot start a stopped connection.");

            try
            {
                TryConnect();
                if (server == null)
                {
                    this.Stop();
                    return;
                }

                while (!isStopped)
                {
                    if (!server.Connected)
                    {
                        Logger.LogWarning("Server is no longer connected.");
                        this.Stop();
                        return;
                    }

                    var socketObject = WaitAndRead();
                    if (socketObject == null)
                        continue;

                    switch (socketObject.Action)
                    {
                        case SocketConstants.SocketAction.Acknowledge:
                            throw new NotSupportedException("Acknowledge not supported at this time.");

                        case SocketConstants.SocketAction.Ping:
                            break;

                        case SocketConstants.SocketAction.Map:
                            Logger.LogDebug("Read Map action.");
                            if (OnMapReceived != null)
                                OnMapReceived(((ImageSocketObject)socketObject).Image);
                            break;

                        case SocketConstants.SocketAction.Fog:
                            Logger.LogDebug("Read Fog action.");
                            if (OnFogReceived != null)
                                OnFogReceived(((ImageSocketObject)socketObject).Image);
                            break;

                        case SocketConstants.SocketAction.FogUpdate:
                            Logger.LogDebug("Read Fog Update action.");
                            if (OnFogUpdateReceived != null)
                                OnFogUpdateReceived(((FogUpdateSocketObject)socketObject).Points, ((FogUpdateSocketObject)socketObject).IsClearing);
                            break;

                        case SocketConstants.SocketAction.GridSize:
                            Logger.LogDebug("Read Grid Size action.");
                            if (OnGridSizeReceived != null)
                                OnGridSizeReceived(((GridSizeSocketObject)socketObject).ShowGrid, ((GridSizeSocketObject)socketObject).GridSize);
                            break;

                        case SocketConstants.SocketAction.GridColor:
                            Logger.LogDebug("Read Grid Color action.");
                            if (OnGridColorReceived != null)
                                OnGridColorReceived(((ColorSocketObject)socketObject).Value);
                            break;

                        case SocketConstants.SocketAction.BlackoutOn:
                            Logger.LogDebug("Read Blackout On action.");
                            if (OnBlackoutReceived != null)
                                OnBlackoutReceived(true);
                            break;
                        case SocketConstants.SocketAction.BlackoutOff:
                            Logger.LogDebug("Read Blackout Off action.");
                            if (OnBlackoutReceived != null)
                                OnBlackoutReceived(false);
                            break;
                        case SocketConstants.SocketAction.Exit:
                            Logger.LogDebug("Read Exit action.");
                            if (OnExitReceived != null)
                                OnExitReceived();
                            this.Stop();
                            break;

                        default:
                            throw new NotImplementedException(string.Format("Socket Action '{0}' is not implemented.", socketObject.Action));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Client Socket - An error occurred connecting to the server.", e);
                this.Stop();
            }
        }
        
        private void TryConnect()
        {
            try
            {
                // Establish the local endpoint for the socket.
                // Dns.GetHostName returns the name of the host running the application.
                var ipAddress = (Utils.IsIPAddress(address)) ? IPAddress.Parse(address) : Dns.Resolve(address).AddressList[0];
                var remoteEndPoint = new IPEndPoint(ipAddress, port);

                Logger.LogDebug(string.Format("Client Socket - Connecting to server at '{0}:{1}'...", ipAddress, port));
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Connect(remoteEndPoint);
                Logger.LogDebug("Client Socket - Connected to server.");

                var ackObject = WaitAndRead();
                if (ackObject == null || ackObject.Action != SocketConstants.SocketAction.Acknowledge)
                    throw new InvalidOperationException("Acknowledge not received.");
                Logger.LogDebug("Client Socket - Acknowledge received.");
            }
            catch (Exception e)
            {
                Logger.LogError("Client Socket - An error occurred trying to establish initial connection to server.", e);
            }
        }

        private BaseSocketObject WaitAndRead()
        {
            var allBytes = new List<byte[]>();
            try
            {
                // Get the first Int32 to find out how many bytes we should expect.
                var bytesExpectedBuffer = new byte[4];
                server.Receive(bytesExpectedBuffer);
                var bytesExpected = BitConverter.ToInt32(bytesExpectedBuffer, 0);
                Logger.LogDebug(string.Format("Expecting {0} total bytes (4 read already, so {1} total read for this message).", bytesExpected, bytesExpected + 4));
                var bytesBuffer = new byte[bytesExpected];
                // Loop until we get all the bytes we're expecting. We should get it in one shot, but it'll depend on how the packet sizes in use.
                var bytesReceived = 0;
                while (bytesReceived < bytesExpected)
                {
                    var thisBytesReceived = server.Receive(bytesBuffer);
                    bytesReceived += thisBytesReceived;
                    allBytes.Add(bytesBuffer.Take(thisBytesReceived).ToArray());
                }

                //while (true)
                //{
                //    var bytesReceived = new byte[1024];
                //    var bytesReceivedCount = server.Receive(bytesReceived);

                //    var actualBytesReceived = (bytesReceivedCount == bytesReceived.Length) ? bytesReceived.ToArray() : bytesReceived.Take(bytesReceivedCount).ToArray();

                //    // If we never received our End Of Data flag, then we must keep going.
                //    allBytes.Add(actualBytesReceived);
                //    if (actualBytesReceived.Last() == (byte)SocketConstants.SocketAction.EndOfData)
                //        break;
                //}

                var allBytesConcat = allBytes.SelectMany(b => b).ToArray();
                Logger.LogDebug(string.Format("Read {0} of expected {1} total bytes.", allBytesConcat.Length, bytesExpected));
                return BaseSocketObject.BaseObjectFromBytes(allBytesConcat);
            }
            catch (Exception e)
            {
                var readBytesCount = allBytes.SelectMany(b => b).Count();
                Logger.LogError(string.Format("Client Socket - An error occurred reading data from the server. Read {0} bytes before failure.", readBytesCount), e);
                return null;
            }
        }

        public void Stop()
        {
            if (isStopped)
                return;

            Logger.LogDebug("Client Socket - Stopping...");
            isStopped = true;

            if (server != null)
            {
                Logger.LogDebug("Client Socket - Closing...");
                server.Close();
                Logger.LogDebug("Client Socket - Closed...");
            }

            Logger.LogDebug("Client Socket - Stopped.");
        }
    }
}
