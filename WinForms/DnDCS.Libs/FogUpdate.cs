using System.Collections.Generic;
using System.Linq;
using DnDCS.Libs.SocketObjects;

namespace DnDCS.Libs
{
    public class FogUpdate
    {
        private readonly LinkedList<DnDPoint> _points = new LinkedList<DnDPoint>();
        public DnDPoint[] Points { get { return _points.ToArray(); } }
        public bool IsClearing { get; set; }
        public int Length { get { return _points.Count; } }

        public FogUpdate(bool isClearing)
            : this(new LinkedList<DnDPoint>(), isClearing)
        {
        }

        public FogUpdate(LinkedList<DnDPoint> points, bool isClearing)
        {
            _points = points ?? new LinkedList<DnDPoint>();
            IsClearing = isClearing;
        }

        public void Add(DnDPoint point)
        {
            _points.AddLast(point);
        }
    }
}