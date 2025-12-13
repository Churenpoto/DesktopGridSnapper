using System;
using System.Windows.Forms;

namespace DesktopGridSnapper
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm { Visible = false });
//            Application.Run(new MainForm());
        }
    }
}
