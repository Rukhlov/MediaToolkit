using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.Encoder.DDATest
{
    class DDATest
    {
        public static void Run()
        {
            Shcore.SetProcessPerMonitorDpiAwareness();

            Console.WriteLine("SystemInformation.VirtualScreen " + SystemInformation.VirtualScreen);


            foreach (var s in Screen.AllScreens)
            {
                Console.WriteLine(s.DeviceName + " " + s.Bounds + " " + s.Primary);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

        }

    }
}
