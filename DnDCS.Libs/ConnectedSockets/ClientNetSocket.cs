using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace DnDCS.Libs.ClientSockets
{
    public class ClientNetSocket : ClientSocket
    {
        private readonly Socket socket;

        protected override IPEndPoint EndPoint { get { return (IPEndPoint)socket.RemoteEndPoint; } }

        public ClientNetSocket(Socket socket)
        {
            this.socket = socket;
        }

        public override void Send(byte[] bytes)
        {
            socket.Send(bytes);
        }

        public override void Dispose()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}
