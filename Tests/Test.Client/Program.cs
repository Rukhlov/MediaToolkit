using MediaToolkit.NativeAPIs;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestClient
{
    static class Program
    {

        private static Logger logger = null;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {

            logger = LogManager.GetCurrentClassLogger();

            AppDomain.CurrentDomain.UnhandledException += (o, a) =>
            {
                Exception ex = null;

                var obj = a.ExceptionObject;
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


                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            };

            logger.Info("========== START ============");

            var winVersion = Environment.OSVersion.Version;
            bool isCompatibleOSVersion = (winVersion.Major >= 6 && winVersion.Minor >= 2);

            if (!isCompatibleOSVersion)
            {
                logger.Fatal("Windows versions earlier than 8 are not supported.");

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            InitMediaLib();

            Shcore.SetProcessPerMonitorDpiAwareness();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm form = new MainForm();
 
            Application.Run(form);

            ShutdownMediaLib();

            logger.Info("========== THE END ============");


            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm());


        }

        public static void InitMediaLib()
        {
            logger.Debug("MainForm::Init()");


            SharpDX.MediaFoundation.MediaManager.Startup();
            SharpDX.Configuration.EnableReleaseOnFinalizer = true;

            WinMM.timeBeginPeriod(1);
        }

        public static void ShutdownMediaLib()
        {
            logger.Debug("MainForm::Shutdown()");

            WinMM.timeEndPeriod(1);
            SharpDX.MediaFoundation.MediaManager.Shutdown();


        }
    }
}
