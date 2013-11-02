﻿using System.Net;
using SuperWebSocket;
using System.Linq;

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
            //var bytesAsString = string.Concat(bytes.Select(b => b.ToString("x2")));
            //socket.Send(bytesAsString);
            socket.Send(bytes, 0, bytes.Length);
        }

        public override void Dispose()
        {
            // TODO: Investigate the other Close methods that might let us transmit a last message saying that the server closed the connection on purpose.
            socket.Close();
        }
    }
}