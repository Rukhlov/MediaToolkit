using CommandLine;
using MediaToolkit.Common;


using NLog;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

using MediaToolkit;
using TestStreamer.Controls;

using MediaToolkit.NativeAPIs;
using MediaToolkit.Core;

namespace TestStreamer
{


    class Program
    {
        private static Logger logger = null;

        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "App started...";

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

            var winVersion = Environment.OSVersion.Version;
            bool isCompatibleOSVersion = (winVersion.Major >= 6 && winVersion.Minor >= 2);

            if (!isCompatibleOSVersion)
            {
                logger.Fatal("Windows versions earlier than 8 are not supported.");

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }


            Shcore.SetDpiAwareness();

            SharpDX.MediaFoundation.MediaManager.Startup();
            SharpDX.Configuration.EnableReleaseOnFinalizer = true;

            MediaToolkit.NativeAPIs.WinMM.timeBeginPeriod(1);

            //Utils.DwmApi.DisableAero(true);


            MainForm form = new MainForm();
     

            Application.Run(form);
            

            MediaToolkit.NativeAPIs.WinMM.timeEndPeriod(1);

            SharpDX.MediaFoundation.MediaManager.Shutdown();

            logger.Info("========== THE END ============");

        }
    }



    public class CommandLineOptions
    {
        [Option("ipaddr")]
        public string IpAddr { get; set; } = "239.0.0.1";

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

        [Option("isim")]
        public bool EnableInputSimulator { get; set; } = true;

        [Option("srcrect")]
        public Rectangle SrcRect { get; set; } = Rectangle.Empty;

        [Option("dstsize")]
        public Size DstSize { get; set; } = Size.Empty;

        [Option("aratio")]
        public bool AspectRatio { get; set; } = true;

        [Option("showmouse")]
        public bool ShowMouse { get; set; } = true;
        //...

    }

    //    [STAThread]
    //    static void _Main(string[] args)
    //    {
    //        Console.Title = "App started...";

    //        logger = LogManager.GetCurrentClassLogger();

    //        AppDomain.CurrentDomain.UnhandledException += (o, a) =>
    //        {
    //            Exception ex = null;

    //            var obj = a.ExceptionObject;
    //            if (obj != null)
    //            {
    //                ex = obj as Exception;
    //                logger.Fatal(ex);

    //            }
    //            if (ex != null)
    //            {
    //                logger.Fatal(ex);
    //            }
    //            else
    //            {
    //                logger.Fatal("FATAL ERROR!!!");
    //            }


    //            Console.WriteLine("Press any key to exit...");
    //            Console.ReadKey();
    //        };

    //        logger.Info("========== START ============");

    //        CommandLineOptions options = null;
    //        if (args != null)
    //        {
    //            logger.Info("Command Line String: " + string.Join(" ", args));

    //            options = new CommandLineOptions();
    //            var res = Parser.Default.ParseArguments(args, options);
    //            if (!res)
    //            {
    //                //...
    //            }
    //        }

    //        var winVersion = Environment.OSVersion.Version;
    //        bool isCompatibleOSVersion = (winVersion.Major >= 6 && winVersion.Minor >= 2);

    //        if (!isCompatibleOSVersion)
    //        {
    //            logger.Fatal("Windows versions earlier than 8 are not supported.");

    //            Console.WriteLine("Press any key to exit...");
    //            Console.ReadKey();
    //        }


    //        Shcore.SetDpiAwareness();

    //        SharpDX.MediaFoundation.MediaManager.Startup();

    //        MediaToolkit.NativeAPIs.WinMM.timeBeginPeriod(1);

    //        //Utils.DwmApi.DisableAero(true);


    //        int fps = 30;
    //        bool aspectRatio = false;

    //        // var srcRect = System.Windows.Forms.Screen.AllScreens[1].Bounds;

    //        var srcRect = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
    //        //var srcRect = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
    //        //var srcRect = SystemInformation.WorkingArea;

    //        //var srcRect = new Rectangle(0, 0, 1280, 1440);
    //        //var srcRect = new Rectangle(0, 0, 2560, 1080);

    //        // var destSize = new Size(1280, 720);
    //        //var destSize = new Size(2560, 1440);
    //        // var destSize = new Size(1920, 1080);

    //        var destSize = new Size(srcRect.Width, srcRect.Height);
    //        if (aspectRatio)
    //        {
    //            var ratio = srcRect.Width / (double)srcRect.Height;
    //            int destWidth = destSize.Width;
    //            int destHeight = (int)(destWidth / ratio);
    //            if (ratio < 1)
    //            {
    //                destHeight = destSize.Height;
    //                destWidth = (int)(destHeight * ratio);
    //            }

    //            destSize = new Size(destWidth, destHeight);
    //        }

    //        ScreenSource source = new ScreenSource();
    //        ScreenCaptureParams captureParams = new ScreenCaptureParams
    //        {
    //            SrcRect = srcRect,
    //            DestSize = destSize,
    //            CaptureType = CaptureType.DXGIDeskDupl,
    //            //CaptureType = CaptureType.Direct3D,
    //            //CaptureType = CaptureType.GDI,
    //            Fps = fps,
    //            CaptureMouse = true,
    //        };

    //        source.Setup(captureParams);

    //        NetworkStreamingParams networkParams = new NetworkStreamingParams
    //        {
    //            DestAddr = options.IpAddr,
    //            DestPort = options.Port,
    //        };

    //        VideoEncodingParams encodingParams = new VideoEncodingParams
    //        {
    //            Width = destSize.Width, // options.Width,
    //            Height = destSize.Height, // options.Height,
    //            FrameRate = options.FrameRate,
    //            EncoderName = "libx264", // "h264_nvenc", //
    //        };

    //        ScreenStreamer videoStreamer = new ScreenStreamer(source);
    //        videoStreamer.Setup(encodingParams, networkParams);

    //        var captureTask = source.Start();
    //        var streamerTask = videoStreamer.Start();


    //        //Thread.Sleep(2000);
    //        /*
    //        NetworkStreamingParams networkParams = new NetworkStreamingParams
    //        {
    //            Address = "0.0.0.0",
    //            Port = 8086,
    //        };

    //        MJpegOverHttpStreamer videoStreamer = new MJpegOverHttpStreamer(source);
    //        VideoEncodingParams encodingParams = new VideoEncodingParams
    //        {
    //            Width = destSize.Width, // options.Width,
    //            Height =  destSize.Height, // options.Height,
    //            FrameRate = options.FrameRate,
    //            EncoderName = "mjpeg",
    //        };

    //        var streamerTask = videoStreamer.Start(encodingParams, networkParams);
    //        */

    //        //AudioLoopbackSource audioStreamer = new AudioLoopbackSource();


    //        /*
    //        var audioParams = new AudioEncodingParams
    //        {
    //            SampleRate = 8000,
    //            Channels = 1,
    //            Encoding = "PCMU"
    //        };

    //        audioStreamer.Start(audioParams);
    //        */

    //        var uiThread = new Thread(() =>
    //        {
    //            Controls.StatisticForm statisticForm = new Controls.StatisticForm();
    //            statisticForm.Start();

    //            PreviewForm previewForm = new PreviewForm();
    //            previewForm.Setup(source);

    //            previewForm.Show();

    //            MainForm form = new MainForm();

    //            Application.Run(form);
    //        });

    //        uiThread.IsBackground = true;
    //        uiThread.SetApartmentState(ApartmentState.STA);


    //        Task.Run(() =>
    //        {
    //            Console.Title = "App running, press 'q' to quit...";


    //            while (Console.ReadKey().Key != ConsoleKey.Q) ;

    //            logger.Debug("'q' pressed...");
    //            //videoStreamer?.Close();

    //            //cancellationTokenSource.Cancel();

    //            videoStreamer?.Close();
    //            source?.Close();
    //        });

    //        Console.CancelKeyPress += (o, a) =>
    //        {
    //            logger.Debug("Ctrl+C pressed...");
    //            a.Cancel = true;

    //            //httpStreamer?.Close();
    //            //source?.Close();

    //            logger.Debug("Environment exit...");
    //            Environment.Exit(0);


    //        };

    //        DesktopManager desktopMan = new DesktopManager();

    //        var desktopControllerTask = desktopMan.Start();

    //        //Application.Run(previewForm);
    //        logger.Info("==========APP RUN ============");

    //        //var task =  TaskEx.WhenAllOrFirstException(new [] { streamerTask, captureTask });

    //        uiThread.Start();

    //        var task = Task.WhenAny(streamerTask, captureTask, desktopControllerTask);

    //        var result = task.Result;
    //        var aex = result.Exception;
    //        if (aex != null)
    //        {
    //            logger.Error(aex);

    //            videoStreamer?.Close();
    //            source?.Close();
    //            desktopMan?.Stop();
    //        }

    //        //var faultedTasks = result.Where(t => t.Exception != null);

    //        //foreach(var t in faultedTasks)
    //        //{
    //        //    var aex = t.Exception;
    //        //    if (aex != null)
    //        //    {
    //        //        logger.Error(aex);
    //        //    }
    //        //}



    //        //Task.WaitAll(streamerTask, captureTask);

    //        MediaToolkit.NativeAPIs.WinMM.timeEndPeriod(1);

    //        SharpDX.MediaFoundation.MediaManager.Shutdown();

    //        logger.Info("========== THE END ============");

    //        Console.Title = "App stopped, press any key to exit...";

    //        Console.ReadKey();


    //    }

    //}


}
