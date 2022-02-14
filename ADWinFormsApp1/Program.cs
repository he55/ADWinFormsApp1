using System;
using System.Windows.Forms;

namespace ADWinFormsApp1
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Window1 window1 = new Window1();
            window1.Show();
            Application.Run();
        }
    }
}
