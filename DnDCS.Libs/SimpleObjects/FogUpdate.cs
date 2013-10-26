using System.Collections.Generic;
using System.Linq;

namespace DnDCS.Libs.SimpleObjects
{
    public class FogUpdate
    {
        private readonly LinkedList<SimplePoint> _points = new LinkedList<SimplePoint>();
        public SimplePoint[] Points { get { return _points.ToArray(); } }
        public bool IsClearing { get; set; }
        public int Length { get { return _points.Count; } }

        public FogUpdate(bool isClearing)
            : this(new SimplePoint[0], isClearing)
        {
        }

        public FogUpdate(IEnumerable<SimplePoint> points, bool isClearing)
        {
            _points = new LinkedList<SimplePoint>();
            foreach (var p in points)
                _points.AddLast(p);

            IsClearing = isClearing;
        }

        public void Add(SimplePoint point)
        {
            _points.AddLast(point);
        }
    }
}