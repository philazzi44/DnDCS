using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DnDCS.Libs.SimpleObjects;
using System.Xml.Serialization;

namespace DnDCS.Libs.PersistenceObjects
{
    public class FogData
    {
        public bool IsClearing { get; set; }
        public int[] Xs { get; set; }
        public int[] Ys { get; set; }

        private SimplePoint[] points;
        [XmlIgnore]
        public SimplePoint[] Points
        {
            get
            {
                if (points == null)
                {
                    points = new SimplePoint[Xs.Length];
                    for (var i = 0; i < Xs.Length; i++)
                        points[i] = new SimplePoint(Xs[i], Ys[i]);
                }

                return points;
            }
            set
            {
                points = value;
                Xs = value.Select(p => p.X).ToArray();
                Ys = value.Select(p => p.Y).ToArray();
            }
        }

        public FogData()
        {
            Xs = new int[0];
            Ys = new int[0];
        }
    }
}
