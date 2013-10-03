using System;
using System.Windows.Forms;
using DnDCS.Libs.SocketObjects;

namespace DnDCS.WinFormsLibs
{
    public interface IDnDCSControl
    {
        MainMenu GetMainMenu();
        DnDPoint ScrollPosition { get; set; }
        Action<bool> ToggleFullScreen { get; set; }
    }
}
