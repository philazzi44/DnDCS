using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnDCS.Libs.PersistenceObjects
{
    public class ServerData
    {
        public bool RealTimeFogUpdates { get; set; }
        public bool ShowGridValues { get; set; }
        public bool ShowLog { get; set; }
        public bool FitMapToScreen { get; set; }

        public string[] ServerImageUrlHistory { get; set; }

        public bool ShowGrid { get; set; }
        public int GridSize { get; set; }
        public bool IsGridColorSet { get; set; }
        public System.Drawing.Color GridColor
        {
            get { return (!IsGridColorSet) ? System.Drawing.Color.Aqua : System.Drawing.Color.FromArgb(GridColorA, GridColorR, GridColorG, GridColorB); }
            set
            {
                IsGridColorSet = true;
                GridColorA = value.A;
                GridColorR = value.R;
                GridColorG = value.G;
                GridColorB = value.B;
            }
        }
        /// <summary> Use GridColor to set. </summary>
        public int GridColorA { get; set; }
        /// <summary> Use GridColor to set. </summary>
        public int GridColorR { get; set; }
        /// <summary> Use GridColor to set. </summary>
        public int GridColorG { get; set; }
        /// <summary> Use GridColor to set. </summary>
        public int GridColorB { get; set; }

        public ServerData()
        {
        }
    }
}
