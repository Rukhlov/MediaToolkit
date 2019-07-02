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
using System.Windows.Forms;

namespace ScreenStreamer
{

    class Program
    {
        private static Logger logger = null;

        [STAThread]
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

            //Utils.DwmApi.DisableAero(true);
            

            ScreenSource source = new ScreenSource();
            
            int fps = 10;
            
            // var srcRect = System.Windows.Forms.Screen.AllScreens[1].Bounds;
            var srcRect = System.Windows.Forms.Screen.PrimaryScreen.Bounds;

            var destSize = new Size(1280, 720);
            // var destSize = new Size(2560, 1440);
            //var destSize = new Size(1920, 1080);

            var ratio = srcRect.Width / (double)srcRect.Height;
            if(ratio > 1)
            {
                int destWidth = srcRect.Width / 2;
                int destHeight =(int)(destWidth / ratio);
                destSize = new Size(destWidth, destHeight);
            }
            else
            {
                int destHeight = srcRect.Height / 2;
                int destWidth = (int)(destHeight * ratio);
                destSize = new Size(destWidth, destHeight);
            }


            source.Start(srcRect, destSize, fps);

            Controls.StatisticForm statisticForm = new Controls.StatisticForm();
            statisticForm.Start();


            VideoEncodingParams encodingParams = new VideoEncodingParams
            {
                Width = destSize.Width, // options.Width,
                Height =  destSize.Height, // options.Height,
                FrameRate = options.FrameRate,
                EncoderName = "", //libx264 // h264_nvenc
            };
            
            MJpegOverHttpStreamer httpStreamer = new MJpegOverHttpStreamer(source);
            httpStreamer.Start(encodingParams);
            
            
            /*
            NetworkStreamingParams networkParams = new NetworkStreamingParams
            {
                MulitcastAddres = options.ServerAddr,
                Port = options.Port,
            };



            VideoMulticastStreamer videoStreamer = new VideoMulticastStreamer(source);
            videoStreamer.Start(encodingParams, networkParams);

            //AudioLoopbackSource audioStreamer = new AudioLoopbackSource();
            */

            /*
            var audioParams = new AudioEncodingParams
            {
                SampleRate = 8000,
                Channels = 1,
                Encoding = "PCMU"
            };

            audioStreamer.Start(audioParams);
            */


            //Console.WriteLine("Press any key to exit...");
            //Console.ReadKey();

            logger.Info("==========APP RUN ============");
            Application.Run();

            //audioStreamer?.Close();
            //videoStreamer?.Close();

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
