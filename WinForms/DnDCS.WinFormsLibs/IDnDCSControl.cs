using System;
using System.Windows.Forms;
using DnDCS.Libs.SimpleObjects;

namespace DnDCS.WinFormsLibs
{
    public interface IDnDCSControl
    {
        MainMenu GetMainMenu();
        SimplePoint ScrollPosition { get; set; }
        Action<bool> ToggleFullScreen { get; set; }
    }
}
