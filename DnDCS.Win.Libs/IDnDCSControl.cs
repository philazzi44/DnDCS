using System;
using System.Windows.Forms;

namespace DnDCS.Win.Libs
{
    public interface IDnDCSControl
    {
        MainMenu GetMainMenu();
        Action<bool> ToggleFullScreen { get; set; }
    }
}
