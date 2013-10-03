using System;
using System.Drawing;
using System.Windows.Forms;

namespace DnDCS.Libs
{
    public interface IDnDCSControl
    {
        MainMenu GetMainMenu();
        Point ScrollPosition { get; set; }
        Action<bool> ToggleFullScreen { get; set; }
    }
}
