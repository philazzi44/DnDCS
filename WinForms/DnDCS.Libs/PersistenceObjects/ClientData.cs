
using DnDCS.Libs.SimpleObjects;
namespace DnDCS.Libs.PersistenceObjects
{
    public class ClientData
    {
        public SimpleServerAddress[] ServerAddressHistory { get; set; }

        public ClientData()
        {
        }
    }
}
