using MediaToolkit;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.Decoder
{
    static class Program
    {

        private static Logger logger = null;

        [STAThread]
        static void Main()
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Info("========== START ============");

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                MediaToolkitManager.Startup();
                Application.Run(new Form1());
            }
            catch(Exception ex)
            {
                logger.Fatal(ex);
            }
            finally
            {
                MediaToolkitManager.Shutdown();

                var dxLog = SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects();
                logger.Info("SharpDx Active Objects:\r\n------------------------------------------------------\r\n" + dxLog);

                logger.Info("========== THE END ============");
            }


            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
