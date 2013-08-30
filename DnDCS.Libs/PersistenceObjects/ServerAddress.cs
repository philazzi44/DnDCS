using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnDCS.Libs.PersistenceObjects
{
    public class ServerAddress : IEquatable<ServerAddress>
    {
        public ServerAddress()
        {
        }

        public string Address { get; set; }
        public int Port { get; set; }

        public override bool Equals(object obj)
        {
            return (obj is ServerAddress && this.Equals((ServerAddress)obj));
        }

        public override int GetHashCode()
        {
            return string.Format("{0}:{1}", Address, Port).GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Address, Port);
        }

        public bool Equals(ServerAddress other)
        {
            return (Address == other.Address && Port == other.Port);
        }
    }
}
