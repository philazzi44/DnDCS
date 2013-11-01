using System;
using System.Net;
using System.Net.Sockets;
using DnDCS.Libs.ClientSockets;

namespace DnDCS.Libs.ServerEvents
{
    public class ServerEvent
    {
        public enum SocketEventType
        {
            ClientConnected,
            ClientDisconnected,
            DataSent,
        }

        private DateTime _time;
        private SocketEventType _eventType;
        private string EventTypeString
        {
            get
            {
                switch (_eventType)
                {
                    case SocketEventType.ClientConnected:
                        return "Client Connected";
                    case SocketEventType.ClientDisconnected:
                        return "Client Disconnected";
                    case SocketEventType.DataSent:
                        return "Data Sent";
                    default:
                        return _eventType.ToString();
                }
            }
        }

        private readonly string _address;
        private readonly string _description;

        public ServerEvent(SocketEventType eventType, string description = null)
        {
            _time = DateTime.Now;
            _eventType = eventType;
            _description = description;
        }

        public ServerEvent(string address, SocketEventType eventType, string description = null)
            : this(eventType, description)
        {
            _address = address;
        }

        public ServerEvent(ClientSocket client, SocketEventType eventType, string description = null)
            : this(eventType, description)
        {
            try
            {
                _address = client.Address;
            }
            catch
            {
                _address = "Unknown";
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(_address))
            {
                return string.IsNullOrWhiteSpace(_description) ? string.Format("{0} @ {1}", EventTypeString, _time) :
                                                                string.Format("{0} @ {1} [{2}]", EventTypeString, _time, _description);
            }

            return string.IsNullOrWhiteSpace(_description) ? string.Format("{0} @ {1} ({2})", EventTypeString, _time, _address) :
                                                            string.Format("{0} @ {1} ({2}) [{3}]", EventTypeString, _time, _address, _description);
        }
    }
}