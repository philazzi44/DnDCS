
namespace DnDCS.Libs.PersistenceObjects
{
    public class ServerData
    {
        public bool RealTimeFogUpdates { get; set; }
        public bool ShowGridValues { get; set; }
        public bool ShowLog { get; set; }

        public string[] ServerImageUrlHistory { get; set; }

        public bool UseFogAlphaEffect { get; set; }

        public bool ShowGrid { get; set; }
        public int GridSize { get; set; }
        public bool IsGridColorSet { get; set; }
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
