using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.IO.Pipes;
using System.Drawing;
using System.IO;
using System.Security.AccessControl;

namespace DnDCS.Libs
{
    public class ClientPipeConnection
    {
        private const int TIMEOUT = 1000;

        private NamedPipeClientStream pipe;
        private readonly Thread clientThread;
        private bool stop;

        public event Action<Image> OnMapReceived;
        public event Action<Image> OnFogReceived;
        public event Action<Image> OnFogUpdateReceived;
        public event Action OnExitReceived;

        public ClientPipeConnection()
        {
            clientThread = new Thread(Start);
        }

        public void StartConnectToServer(string address)
        {
            //var security = new PipeSecurity();
            //security.AddAccessRule(new PipeAccessRule("pazzi", PipeAccessRights.Read, AccessControlType.Allow));
            pipe = new NamedPipeClientStream(address, PipeConstants.PIPE_NAME, PipeDirection.In, PipeOptions.None);
            //pipe.SetAccessControl(security);

            clientThread.Start();
        }

        private void Start()
        {
            try
            {
                // Attempt to connect to the server.
                pipe.Connect(TIMEOUT);
                var ack = Read();
                if (ack != PipeConstants.PipeAction.ACK)
                    throw new InvalidOperationException("ACK not received.");

                Logger.LogDebug("Client Connection - Received Ack.");

                try
                {
                    // Continue reading until we're told to stop.
                    while (!stop)
                    {
                        byte[] dataBytes;
                        var pipeAction = Read(out dataBytes);
                        switch (pipeAction)
                        {
                            case PipeConstants.PipeAction.ACK:
                                throw new NotSupportedException("ACK not supported at this time.");

                            case PipeConstants.PipeAction.EXIT:
                                if (OnExitReceived != null)
                                    OnExitReceived();
                                break;

                            case PipeConstants.PipeAction.MAP:
                                if (OnMapReceived != null)
                                    OnMapReceived(ConvertBytesToImage(dataBytes));
                                break;

                            case PipeConstants.PipeAction.FOG:
                                if (OnFogReceived != null)
                                    OnFogReceived(ConvertBytesToImage(dataBytes));
                                break;

                            case PipeConstants.PipeAction.FOG_UPDATE:
                                if (OnFogUpdateReceived != null)
                                    OnFogUpdateReceived(ConvertBytesToImage(dataBytes));
                                break;
                        }
                    }
                }
                catch (ThreadInterruptedException e)
                {
                    Logger.LogWarning("Client Connection - Thread Interrupted while trying to connect to server.", e);
                }
                catch (Exception e)
                {
                    Logger.LogError("Client Connection - Failed to parse received message.", e);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Client Connection - Failed to start Client Connection.", e);
            }
        }

        public PipeConstants.PipeAction Read()
        {
            byte[] dataBytes;
            return Read(out dataBytes);
        }

        public PipeConstants.PipeAction Read(out byte[] dataBytes)
        {
            Logger.LogDebug("Client Connection - Waiting for data on the pipe...");
            var pipeAction = (PipeConstants.PipeAction)(byte)pipe.ReadByte();
            Logger.LogDebug(string.Format("Client Connection - Pipe Action '{0}' received.", pipeAction));
            switch (pipeAction)
            {
                case PipeConstants.PipeAction.ACK:
                    dataBytes = null;
                    break;

                case PipeConstants.PipeAction.MAP:
                case PipeConstants.PipeAction.FOG:
                case PipeConstants.PipeAction.FOG_UPDATE:
                    // The next Int32 will be the size of the data bytes following it.
                    Logger.LogDebug("Client Connection - Reading Data Size from pipe...");
                    var messageSizeBuffer = new byte[4];
                    pipe.Read(messageSizeBuffer, 0, messageSizeBuffer.Length);

                    var messageSize = BitConverter.ToInt32(messageSizeBuffer, 0);
                    Logger.LogDebug(string.Format("Client Connection - {0} bytes expected on pipe. Reading....", messageSize));

                    dataBytes = new byte[messageSize];
                    var actualReadBytes = pipe.Read(dataBytes, 0, messageSize);
                    Logger.LogDebug(string.Format("Client Connection - {0} bytes read from pipe ({1} expected).", actualReadBytes, messageSize));
                    break;


                case PipeConstants.PipeAction.EXIT:
                    dataBytes = null;
                    break;

                default:
                    throw new NotImplementedException(string.Format("Pipe Action '{0}' is not implemented.", pipeAction));
            }

            return pipeAction;
        }

        private Image ConvertBytesToImage(byte[] dataBytes)
        {
            using (var ms = new MemoryStream(dataBytes))
            {
                return Image.FromStream(ms);
            }
        }

        public void Stop()
        {
            Logger.LogDebug("Client Connection - Stopping...");
            stop = true;
            clientThread.Interrupt();
            clientThread.Join();
            Logger.LogDebug("Client Connection - Stopped.");
        }
    }
}
