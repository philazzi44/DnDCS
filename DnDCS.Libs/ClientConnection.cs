using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Pipes;
using System.Drawing;
using System.IO;

namespace DnDCS.Libs
{
    public class ClientConnection
    {
        private const int TIMEOUT = 1000;

        private NamedPipeClientStream pipe;
        private readonly Thread clientThread;
        private bool stop;

        public event Action<Image> OnMapReceived;
        public event Action<Image> OnFogReceived;
        public event Action<Image> OnFogUpdateReceived;
        public event Action OnExitReceived;

        public ClientConnection()
        {
            clientThread = new Thread(Start);
        }

        public void ConnectToServer(string address)
        {
            pipe = new NamedPipeClientStream(address, PipeConstants.PIPE_NAME, PipeDirection.In);
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
                catch (ThreadInterruptedException)
                {
                }
                catch (Exception)
                {
                    // Log this bad boy.
                }
            }
            catch (Exception)
            {
                // Log this bad boy.
            }
        }

        public PipeConstants.PipeAction Read()
        {
            byte[] dataBytes;
            return Read(out dataBytes);
        }

        public PipeConstants.PipeAction Read(out byte[] dataBytes)
        {
            var pipeAction = (PipeConstants.PipeAction)(byte)pipe.ReadByte();
            switch (pipeAction)
            {
                case PipeConstants.PipeAction.ACK:
                    dataBytes = null;
                    break;

                case PipeConstants.PipeAction.MAP:
                case PipeConstants.PipeAction.FOG:
                case PipeConstants.PipeAction.FOG_UPDATE:
                    // The next Int32 will be the size of the data bytes following it.
                    var messageSizeBuffer = new byte[4];
                    pipe.Read(messageSizeBuffer, 0, messageSizeBuffer.Length);

                    var messageSize = BitConverter.ToInt32(messageSizeBuffer, 0);
                    dataBytes = new byte[messageSize];
                    pipe.Read(dataBytes, 0, messageSize);
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
            stop = true;
            clientThread.Interrupt();
            clientThread.Join();
        }
    }
}
