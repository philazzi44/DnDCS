using System;
using System.Collections.Generic;

namespace DnDCS.Libs.SimpleObjects
{
    public class GridSizeSocketObject : BaseSocketObject
    {
        public bool ShowGrid { get; private set; }
        public int GridSize { get; private set; }
        
        public GridSizeSocketObject(bool showGrid, int gridSize)
            : base(SocketConstants.SocketAction.GridSize)
        {
            ShowGrid = showGrid;
            GridSize = gridSize;
        }

        public static GridSizeSocketObject GridSizeObjectFromBytes(byte[] bytes)
        {
            var action = (SocketConstants.SocketAction)bytes[0];
            switch (action)
            {
                case SocketConstants.SocketAction.GridSize:
                    return new GridSizeSocketObject(BitConverter.ToBoolean(bytes, 1), BitConverter.ToInt32(bytes, 2));

                default:
                    throw new NotSupportedException(string.Format("Action '{0}' is not supported.", action));
            }
        }

        public override byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.Add(ActionByte);
            bytes.AddRange(BitConverter.GetBytes(ShowGrid));
            bytes.AddRange(BitConverter.GetBytes(GridSize));
            return bytes.ToArray();
        }

        public override string ToString()
        {
            return string.Format("Socket Action: '{0}', Show Grid: {1}, Grid Size: {2}", Action, ShowGrid, GridSize);
        }
    }
}
