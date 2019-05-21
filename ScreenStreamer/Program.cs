using CommandLine;
using CommonData;
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

            CommandLineOptions options = null;
            if (args != null)
            {
                logger.Info("Command Line String: " + string.Join(" ", args));

                options = new CommandLineOptions();
                var res = Parser.Default.ParseArguments(args, options);
                if (!res)
                {
                    //...
                }
            }


            VideoBuffer buffer = new VideoBuffer(options.Width, options.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            ScreenSource source = new ScreenSource();
            source.Start(buffer);

            VideoEncodingParams encodingParams = new VideoEncodingParams
            {
                Width = options.Width,
                Height = options.Height,
                FrameRate = options.FrameRate,
                EncoderName = "",
            };

            NetworkStreamingParams networkParams = new NetworkStreamingParams
            {
                MulitcastAddres = options.ServerAddr,
                Port = options.Port,
            };

            ScreenStreamer streamer = new ScreenStreamer(source);
            streamer.Start(encodingParams, networkParams);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            streamer?.Close();
            source?.Close();

            logger.Info("========== THE END ============");


        }

    }


    public class CommandLineOptions
    {
        [Option("addr")]
        public string ServerAddr { get; set; } = "239.0.0.1";

        [Option("port")]
        public int Port { get; set; } = 1234;

        [Option("width")]
        public int Width { get; set; } = 1920;

        [Option("height")]
        public int Height { get; set; } = 1080;

        [Option("fps")]
        public int FrameRate { get; set; } = 30;

        [Option("encoder")]
        public string EncoderName { get; set; } = "";

        //...

    }
}
