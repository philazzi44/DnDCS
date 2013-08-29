using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DnDCS.Libs.PersistenceObjects
{
    public class ClientData
    {
        public ClientData()
        {
        }

        public ServerAddress[] ServerAddressHistory { get; set; }
    }
}
