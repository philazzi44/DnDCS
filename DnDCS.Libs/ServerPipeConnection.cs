using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Net.Sockets;
using System.IO.Pipes;
using System.Threading;
using System.Drawing;
using System.IO;

namespace DnDCS.Libs
{
    public class ServerPipeConnection
    {
        private readonly NamedPipeServerStream pipe;

        public ServerPipeConnection()
        {
            //var security = new PipeSecurity();
            //security.AddAccessRule(new PipeAccessRule(System.Security.Principal.WindowsIdentity.GetCurrent().User, PipeAccessRights.FullControl, AccessControlType.Allow));
            //security.AddAccessRule(new PipeAccessRule(System.Security.Principal.WindowsIdentity.GetCurrent().User, PipeAccessRights.Write | PipeAccessRights.CreateNewInstance, AccessControlType.Allow));
            pipe = new NamedPipeServerStream(PipeConstants.PIPE_NAME, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            //pipe.SetAccessControl(security);

            // Wait for a client to connect
            Logger.LogDebug("Server Connection - Waiting for client connection.");
            pipe.BeginWaitForConnection(WaitForConnectionResult, null);
        }

        private void WaitForConnectionResult(object state)
        {
            if (!pipe.IsConnected)
            {
                Logger.LogError("Server Connection - Connection callback invoked but pipe is not connected.");
                this.Stop();
                return;
            }

            try
            {
                Write(PipeConstants.PipeAction.ACK);
                pipe.WaitForPipeDrain();
            }
            catch (Exception e)
            {
                Logger.LogError("Server Connection - Failed to send initial ACK message. Stopping Server Connection...", e);
                this.Stop();
            }
        }

        public void WriteMap(Image map)
        {
            Write(PipeConstants.PipeAction.MAP, ConvertImageToBytes(map));
        }

        public void WriteFog(Image fog)
        {
            Write(PipeConstants.PipeAction.FOG, ConvertImageToBytes(fog));
        }

        public void WriteFogUpdate(Image fogUpdate)
        {
            Write(PipeConstants.PipeAction.FOG_UPDATE, ConvertImageToBytes(fogUpdate));
        }

        private void Write(PipeConstants.PipeAction pipeAction, byte[] dataBytes = null)
        {
            if (pipe == null)
                throw new ObjectDisposedException("Underlying pipe has already been closed.");

            try
            {
                Logger.LogDebug(string.Format("Server Connection - Writing Pipe Action '{0}'.", pipeAction));
                pipe.WriteByte(1);
                pipe.WriteByte((byte)pipeAction);
                if (dataBytes != null)
                {
                    Logger.LogDebug(string.Format("Server Connection - Writing {0} Bytes.", dataBytes.Length));

                    var lengthInBytes = BitConverter.GetBytes(dataBytes.Length);
                    foreach (var lengthByte in lengthInBytes)
                        pipe.WriteByte(lengthByte);
                    foreach (var dataByte in dataBytes)
                        pipe.WriteByte(dataByte);
                }
                Logger.LogDebug(string.Format("Server Connection - Waiting for client to read bytes..."));
                pipe.WaitForPipeDrain();
                Logger.LogDebug(string.Format("Server Connection - Bytes read by client."));
            }
            catch (Exception e)
            {
                Logger.LogError(string.Format("Server Connection - Failed to write Pipe Action '{0}'.", pipeAction), e);
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
            Logger.LogDebug("Server Connection - Stopping...");

            // The Pipe Thread may exist waiting on the Pipe, but closing it will interrupt any blocking actions and let the thread die anyways, so we don't
            // need to explicitly stop the thread.
            if (pipe != null)
            {
                if (pipe.IsConnected)
                {
                    Logger.LogDebug("Server Connection - Sending 'Exit' to client...");
                    Write(PipeConstants.PipeAction.EXIT);
                    Logger.LogDebug("Server Connection - 'Exit' Sent to client.");
                }

                Logger.LogDebug("Server Connection - Closing pipe...");
                pipe.Close();
                Logger.LogDebug("Server Connection - Pipe closed.");
            }

            Logger.LogDebug("Server Connection - Stopped.");
        }
    }
}
