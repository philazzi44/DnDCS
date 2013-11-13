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
            NetClientConnected,
            WebClientConnected,
            NetClientDisconnected,
            WebClientDisconnected,
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
                    case SocketEventType.NetClientConnected:
                        return "Net Client Connected";
                    case SocketEventType.WebClientConnected:
                        return "Web Client Connected";
                    case SocketEventType.NetClientDisconnected:
                        return "Net Client Disconnected";
                    case SocketEventType.WebClientDisconnected:
                        return "Web Client Disconnected";
                    case SocketEventType.DataSent:
                        return "Data Sent";
                    default:
                        return _eventType.ToString();
                }
            }
        }

        private readonly string _address;
        private readonly string _description;

        private DnDCS.Libs.SimpleObjects.SocketConstants.SocketAction? _socketAction;

        private ServerEvent()
        {
            _time = DateTime.Now;
        }

        private ServerEvent(string description = null) : this()
        {
            _description = description;
        }

        private ServerEvent(SocketEventType eventType, string description = null)
            : this(description)
        {
            _eventType = eventType;
        }

        private ServerEvent(DnDCS.Libs.SimpleObjects.SocketConstants.SocketAction socketAction)
            : this(SocketEventType.DataSent)
        {
            _socketAction = socketAction;
        }

        public ServerEvent(string address, SocketEventType eventType, string description = null)
            : this(eventType, description)
        {
            _address = address;
        }

        public ServerEvent(string address, DnDCS.Libs.SimpleObjects.SocketConstants.SocketAction socketAction)
            : this(socketAction)
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

        public ServerEvent(ClientSocket client, DnDCS.Libs.SimpleObjects.SocketConstants.SocketAction socketAction)
            : this(socketAction)
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
            var eventTypeOrSocketActionString = (_socketAction.HasValue) ? _socketAction.ToString() : EventTypeString;
            var descriptionString = (string.IsNullOrWhiteSpace(_description)) ? string.Empty : string.Format("[{0}]", _description);

            if (string.IsNullOrWhiteSpace(_address))
                return string.Format("{0} @ {1} {2}", eventTypeOrSocketActionString, _time.ToString("hh:mm:ss:ffffff"), descriptionString).Trim();
            else
                return string.Format("{0} @ {1} ({2}) {3}", eventTypeOrSocketActionString, _time.ToString("hh:mm:ss:ffffff"), _address, descriptionString).Trim();
        }
    }
}