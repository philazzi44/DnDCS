using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace DnDCS.Libs.ClientSockets
{
    public abstract class ClientSocket : IDisposable
    {
        protected abstract IPEndPoint EndPoint { get; }

        public string Address
        {
            get { return EndPoint.ToString(); }
        }

        public abstract void Send(byte[] bytes);

        public abstract void Dispose();
    }
}
