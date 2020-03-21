using MediaToolkit;
using MediaToolkit.NativeAPIs;
using NLog;
using ScreenStreamer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenStreamer.WinForms.App
{
    class Program
    {
        private static Logger logger = null;

        [STAThread]
        static void Main(string[] args)
        {
            //Console.Title = "App started...";

            logger = LogManager.GetCurrentClassLogger();

            logger.Info("========== START ============");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            bool createdNew = false;
            Mutex mutex = null;
            try
            {
                mutex = new Mutex(true, AppConsts.ApplicationId, out createdNew);
                if (!createdNew)
                {
                    logger.Info("Another instance is already running...");
                    //...
                }

                if (args != null && args.Length > 0)
                {//...


                }

                bool tempMode = !createdNew;
                Config.Initialize(tempMode);

                MediaToolkitManager.Startup();

                //DwmApi.DisableAero(true);
                Shcore.SetProcessPerMonitorDpiAwareness();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                MainForm form = new MainForm();

                Application.Run(form);

            }
            finally
            {
                Config.Shutdown();

                MediaToolkitManager.Shutdown();

                if (mutex != null)
                {
                    if (createdNew)
                    {
                        mutex.ReleaseMutex();
                    }
                    mutex.Dispose();
                }



                logger.Info("========== THE END ============");
            }

        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = null;

            var obj = e.ExceptionObject;
            if (obj != null)
            {
                ex = obj as Exception;
                logger.Fatal(ex);

            }
            if (ex != null)
            {
                logger.Fatal(ex);
            }
            else
            {
                logger.Fatal("FATAL ERROR!!!");
            }

            // MessageBox.Show();
        }
    }


}
