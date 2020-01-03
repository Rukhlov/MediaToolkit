using MediaToolkit;
using MediaToolkit.NativeAPIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.VideoRenderer
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            MediaToolkitManager.Startup();

            //DwmApi.DisableAero(true);
            Shcore.SetProcessPerMonitorDpiAwareness();


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());



        }
    }
}
