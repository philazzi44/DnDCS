using System.Collections.Generic;
using System.Linq;
using DnDCS.Libs.SimpleObjects;

namespace DnDCS.Libs
{
    public class FogUpdate
    {
        private readonly LinkedList<SimplePoint> _points = new LinkedList<SimplePoint>();
        public SimplePoint[] Points { get { return _points.ToArray(); } }
        public bool IsClearing { get; set; }
        public int Length { get { return _points.Count; } }

        public FogUpdate(bool isClearing)
            : this(new LinkedList<SimplePoint>(), isClearing)
        {
        }

        public FogUpdate(LinkedList<SimplePoint> points, bool isClearing)
        {
            _points = points ?? new LinkedList<SimplePoint>();
            IsClearing = isClearing;
        }

        public void Add(SimplePoint point)
        {
            _points.AddLast(point);
        }
    }
}