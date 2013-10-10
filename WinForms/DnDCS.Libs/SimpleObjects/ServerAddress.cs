using System;

namespace DnDCS.Libs.SimpleObjects
{
    public class SimpleServerAddress : IEquatable<SimpleServerAddress>
    {
        public SimpleServerAddress()
        {
        }

        public string Address { get; set; }
        public int Port { get; set; }

        public override bool Equals(object obj)
        {
            return (obj is SimpleServerAddress && this.Equals((SimpleServerAddress)obj));
        }

        public override int GetHashCode()
        {
            return string.Format("{0}:{1}", Address, Port).GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Address, Port);
        }

        public bool Equals(SimpleServerAddress other)
        {
            return (Address == other.Address && Port == other.Port);
        }
    }
}
