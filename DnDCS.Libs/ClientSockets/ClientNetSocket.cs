using System.Net;
using System.Net.Sockets;

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
