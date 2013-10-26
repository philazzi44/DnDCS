﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DnDCS.Libs.SimpleObjects;

namespace DnDCS.Libs
{
    public class ClientSocketConnection
    {
        private const string ErrorStartingStoppedConnection = "Client Connection - Cannot start a stopped connection.";

        private readonly Thread socketThread;

        private Socket server;

        public string Address { get { return address; } }
        public int Port { get { return port; } }
        private readonly string address;
        private readonly int port;
        private bool isStopped;

        public event Action OnConnectionEstablished;
        public event Action OnServerNotFound;
        public event Action<SimpleImage> OnMapReceived;
        public event Action<SimplePoint> OnCenterMapReceived;
        public event Action<SimpleImage> OnFogReceived;
        public event Action<FogUpdate> OnFogUpdateReceived;
        public event Action<bool> OnUseFogAlphaEffectReceived;
        public event Action<bool, int> OnGridSizeReceived;
        public event Action<SimpleColor> OnGridColorReceived;
        public event Action OnExitReceived;
        public event Action<bool> OnBlackoutReceived;

        public ClientSocketConnection(string address, int port)
        {
            this.address = address;
            this.port = port;

            socketThread = new Thread(SocketThreadStart);
            socketThread.IsBackground = true;
            socketThread.Name = "Client Socket Thread";
        }

        public void Start()
        {
            if (isStopped)
                throw new ObjectDisposedException(ErrorStartingStoppedConnection);
            socketThread.Start();
        }

        private void SocketThreadStart()
        {
            if (isStopped)
                throw new ObjectDisposedException(ErrorStartingStoppedConnection);

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

                        case SocketConstants.SocketAction.CenterMap:
                            Logger.LogDebug("Read Center Map action.");
                            if (OnCenterMapReceived != null)
                                OnCenterMapReceived(((CenterMapSocketObject)socketObject).CenterMap);
                            break;

                        case SocketConstants.SocketAction.Fog:
                            Logger.LogDebug("Read Fog action.");
                            if (OnFogReceived != null)
                                OnFogReceived(((ImageSocketObject)socketObject).Image);
                            break;

                        case SocketConstants.SocketAction.FogUpdate:
                            Logger.LogDebug("Read Fog Update action.");
                            if (OnFogUpdateReceived != null)
                                OnFogUpdateReceived(((FogUpdateSocketObject)socketObject).FogUpdateInstance);
                            break;

                        case SocketConstants.SocketAction.UseFogAlphaEffect:
                            Logger.LogDebug("Read Use Fog Alpha Effect action.");
                            if (OnUseFogAlphaEffectReceived != null)
                                OnUseFogAlphaEffectReceived(((UseFogAlphaEffectSocketObject)socketObject).UseFogAlphaEffect);
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
                if (OnConnectionEstablished != null)
                    OnConnectionEstablished();
            }
            catch (Exception e)
            {
                Logger.LogError("Client Socket - An error occurred trying to establish initial connection to server.", e);
                if (OnServerNotFound != null)
                    OnServerNotFound();
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
                Logger.LogDebug(string.Format("Expecting {0} more bytes (4 read already, so {1} total read for this message).", bytesExpected, bytesExpected + 4));
                var bytesBuffer = new byte[bytesExpected];
                // Loop until we get all the bytes we're expecting. We should get it in one shot, but it'll depend on how the packet sizes in use.
                var bytesReceived = 0;
                while (bytesReceived < bytesExpected)
                {
                    // Read either a full buffer's worth, or just up to how many we're expecting. This prevents data that is enqueued
                    // behind this object from being read in and corrupting the data.
                    var bytesToRead = Math.Min(bytesExpected - bytesReceived, bytesBuffer.Length);
                    var thisBytesReceived = server.Receive(bytesBuffer, bytesToRead, SocketFlags.None);
                    if (thisBytesReceived > bytesToRead)
                    {
                        throw new InvalidOperationException(string.Format("Socket has been corrupted in some way, as a maximum of {0} bytes were requested but {1} bytes were read.", bytesToRead, thisBytesReceived));
                    }
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
