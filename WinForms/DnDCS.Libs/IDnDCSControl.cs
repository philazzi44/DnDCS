using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace DnDCS.Libs
{
    public interface IDnDCSControl
    {
        MainMenu GetMainMenu();
        Point ScrollPosition { get; set; }
        Action<bool> ToggleFullScreen { get; set; }
    }
}
