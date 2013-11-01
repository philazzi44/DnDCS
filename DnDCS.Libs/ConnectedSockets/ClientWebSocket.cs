using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket;
using System.Net;

namespace DnDCS.Libs.ClientSockets
{
    public class ClientWebSocket : ClientSocket
    {
        private readonly WebSocketSession socket;

        protected override IPEndPoint EndPoint { get { return socket.RemoteEndPoint; } }

        public ClientWebSocket(WebSocketSession socket)
        {
            this.socket = socket;
        }

        public override void Send(byte[] bytes)
        {
            socket.Send(bytes, 0, bytes.Length);
        }

        public override void Dispose()
        {
            // TODO: Investigate the other Close methods that might let us transmit a last message saying that the server closed the connection on purpose.
            socket.Close();
        }
    }
}
