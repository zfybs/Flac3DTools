using System;
using System.Diagnostics;
using System.Windows.Forms;
using Hm2Flac3D.Enhanced;
using Hm2Flac3D.Utility;

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

        private static void TestCentroid()
        {
            XYZ[] nodes = new XYZ[] {
                new XYZ(4.30366, 96.50780,  -17.10120    ),
                new XYZ(3.90772, 96.0923,        -17      ),
                new XYZ(2.82843, 97.17160, -17.00000),
                new XYZ(3.23452, 97.5777,       -17.101  )};

            XYZ cent4 = new XYZ(3.56858, 96.83730, -17.05060);

            XYZ cent31 = new XYZ(3.67993, 96.5906, -17.0337);
            XYZ cent32 = new XYZ(3.45554, 97.08570, -17.06750);
            //
            XYZ c31 = XYZ.FindCentroid(nodes[0], nodes[1], nodes[2]);
            XYZ c32 = XYZ.FindCentroid(nodes[0], nodes[2], nodes[3]);
            XYZ c4 = XYZ.FindCentroid(nodes[0], nodes[1], nodes[2], nodes[3]);
        }
        private static void TestCentroid2()
        {

            XYZ[] nodes = new XYZ[] {
                new XYZ(50.00000,  1.10858,    -13.00000  ),
                new XYZ(49.4261,    1.12248,    -12.8988   ),
                new XYZ(49.41470,   -1.00974E-28,    -12.94240  ),
                new XYZ(50, -1.01E-28,  -13            )};

            XYZ cent31 = new XYZ(49.613600, 0.743687, -12.947100);
            XYZ cent32 = new XYZ(49.804900, 0.369526, -12.980800);

            XYZ cent4 = new XYZ(49.71020, 0.55777, -12.96030);

            //
            XYZ c31 = XYZ.FindCentroid(nodes[0], nodes[1], nodes[2]);
            XYZ c32 = XYZ.FindCentroid(nodes[0], nodes[2], nodes[3]);
            XYZ c4 = XYZ.FindCentroid(nodes[0], nodes[1], nodes[2], nodes[3]);
        }
    }
}