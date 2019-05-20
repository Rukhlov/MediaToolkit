using FFmpegWrapper;

using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenStreamer
{

    class Program
    {
        private static Logger logger = null;
        static void Main(string[] args)
        {

            logger = LogManager.GetCurrentClassLogger();

            AppDomain.CurrentDomain.UnhandledException += (o, a) => 
            {
                //...
                var obj = a.ExceptionObject;
                if (obj != null)
                {
                    
                }
                logger.Fatal("FATAL ERROR!!!");
            };

            logger.Info("========== START ============");

            ScreenStreamer screenStreamer = new ScreenStreamer();

            screenStreamer.Run();

            Console.ReadKey();

            logger.Info("========== THE END ============");


        }

    }
}
