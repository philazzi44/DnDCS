using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DnDCS.Libs
{
    public class FogUpdate
    {
        private readonly LinkedList<Point> _points = new LinkedList<Point>();
        public Point[] Points { get { return _points.ToArray(); } }
        public bool IsClearing { get; set; }
        public int Length { get { return _points.Count; } }

        public FogUpdate(bool isClearing)
            : this(new LinkedList<Point>(), isClearing)
        {
        }

        public FogUpdate(LinkedList<Point> points, bool isClearing)
        {
            _points = points ?? new LinkedList<Point>();
            IsClearing = isClearing;
        }

        public void Add(Point point)
        {
            _points.AddLast(point);
        }
    }
}