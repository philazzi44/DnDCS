using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnDCS.Libs.PersistenceObjects
{
    public class ServerAddress
    {
        public ServerAddress()
        {
        }

        public string Address { get; set; }
        public int Port { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Address, Port);
        }
    }
}
