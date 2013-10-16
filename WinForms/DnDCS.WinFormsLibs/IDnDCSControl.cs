using System;
using System.Windows.Forms;
using DnDCS.Libs.SimpleObjects;

namespace DnDCS.WinFormsLibs
{
    public interface IDnDCSControl
    {
        MainMenu GetMainMenu();
        Action<bool> ToggleFullScreen { get; set; }
    }
}
