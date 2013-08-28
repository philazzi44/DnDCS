using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO.Pipes;
using System.Threading;
using System.Drawing;
using System.IO;

namespace DnDCS.Libs
{
    public class ServerConnection : IDisposable
    {
        private readonly NamedPipeServerStream pipe;
        private Thread serverThread;
        private bool isConnected;

        public ServerConnection()
        {
            pipe = new NamedPipeServerStream(PipeConstants.PIPE_NAME, PipeDirection.Out);
            serverThread = new Thread(Start);
            serverThread.Start();
        }

        private void Start()
        {
            try
            {
                // Wait for a client to connect
                pipe.WaitForConnection();

                isConnected = true;

                Write(PipeConstants.PipeAction.ACK);
                pipe.WaitForPipeDrain();
            }
            catch (ThreadInterruptedException)
            {
                this.Stop();
            }
            catch (Exception)
            {
                this.Stop();
            }
            serverThread = null;
        }

        public void WriteMap(Image map)
        {
            Write(PipeConstants.PipeAction.MAP, ConvertImageToBytes(map));
        }

        public void WriteFog(Image fog)
        {
            Write(PipeConstants.PipeAction.MAP, ConvertImageToBytes(fog));
        }

        public void WriteFogUpdate(Image fogUpdate)
        {
            Write(PipeConstants.PipeAction.MAP, ConvertImageToBytes(fogUpdate));
        }

        private void Write(PipeConstants.PipeAction pipeAction, byte[] dataBytes = null)
        {
            if (!isConnected)
                return;

            try
            {
                pipe.WriteByte(1);
                pipe.WriteByte((byte)pipeAction);
                if (dataBytes != null)
                {
                    var lengthInBytes = BitConverter.GetBytes(dataBytes.Length);
                    foreach (var lengthByte in lengthInBytes)
                        pipe.WriteByte(lengthByte);
                    foreach (var dataByte in dataBytes)
                        pipe.WriteByte(dataByte);
                }
                pipe.WaitForPipeDrain();
            }
            catch (Exception)
            {
                // TODO: Log this.
            }
        }

        private byte[] ConvertImageToBytes(Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        public void Stop()
        {
            if (serverThread.IsAlive)
            {
                serverThread.Interrupt();
                serverThread.Join();
                serverThread = null;
            }

            if (isConnected)
            {
                Write(PipeConstants.PipeAction.EXIT);
                pipe.Close();
                isConnected = false;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
