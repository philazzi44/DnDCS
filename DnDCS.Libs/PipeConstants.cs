using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnDCS.Libs
{
    [Obsolete]
    public static class PipeConstants
    {
        public const string PIPE_NAME = "DnDCS-MainPipe";

        public enum PipeAction : byte
        {
            ACK = 44,
            MAP,
            FOG,
            FOG_UPDATE,
            EXIT,
        }
    }
}
