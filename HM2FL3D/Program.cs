using System;
using System.Windows.Forms;
using Hm2Flac3D.Enhanced;

namespace Hm2Flac3D
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new form_Hm2Flac3D());
        }
    }
}
