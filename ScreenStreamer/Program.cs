﻿using CommandLine;
using CommonData;
using FFmpegWrapper;

using NLog;
using ScreenStreamer.Controls;
using ScreenStreamer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using WindowsInput;

namespace ScreenStreamer
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

            Shcore.SetDpiAwareness();

            SharpDX.MediaFoundation.MediaManager.Startup();

            Utils.WinMM.timeBeginPeriod(1);

            //Utils.DwmApi.DisableAero(true);


            int fps = 30;
            bool aspectRatio = false;

           //  var srcRect = System.Windows.Forms.Screen.AllScreens[1].Bounds;

            var srcRect = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            //var srcRect = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            //var srcRect = SystemInformation.WorkingArea;

            //var srcRect = new Rectangle(0, 0, 1280, 1440);
            //var srcRect = new Rectangle(0, 0, 2560, 1080);

           // var destSize = new Size(1280, 720);
            //var destSize = new Size(2560, 1440);
            // var destSize = new Size(1920, 1080);

            var destSize = new Size(srcRect.Width, srcRect.Height);
            if (aspectRatio)
            {
                var ratio = srcRect.Width / (double)srcRect.Height;
                int destWidth = destSize.Width;
                int destHeight = (int)(destWidth / ratio);
                if (ratio < 1)
                {
                    destHeight = destSize.Height;
                    destWidth = (int)(destHeight * ratio);
                }

                destSize = new Size(destWidth, destHeight);
            }

            ScreenSource source = new ScreenSource();
            ScreenCaptureParams captureParams = new ScreenCaptureParams
            {
                SrcRect = srcRect,
                DestSize = destSize,
                CaptureType = CaptureType.DXGIDeskDupl,
                //CaptureType = CaptureType.Direct3D,
                //CaptureType = CaptureType.GDI,
                Fps = fps,
                CaptureMouse = true,
            };

            source.Setup(captureParams);

            
            //var imageProvider = new D3DImageProvider();
            //imageProvider.Setup(source);

            //PreviewForm previewForm = new PreviewForm();
            //previewForm.SetProvider(imageProvider);

         
    
            NetworkStreamingParams networkParams = new NetworkStreamingParams
            {
                Address = options.ServerAddr,
                Port = options.Port,
            };

            VideoEncodingParams encodingParams = new VideoEncodingParams
            {
                Width = destSize.Width, // options.Width,
                Height = destSize.Height, // options.Height,
                FrameRate = options.FrameRate,
                EncoderName = "libx264", // "h264_nvenc", //
            };

            VideoMulticastStreamer videoStreamer = new VideoMulticastStreamer(source);
            videoStreamer.Setup(encodingParams, networkParams);

            var captureTask = source.Start();
            var streamerTask = videoStreamer.Start();


            //Thread.Sleep(2000);
            /*
            NetworkStreamingParams networkParams = new NetworkStreamingParams
            {
                Address = "0.0.0.0",
                Port = 8086,
            };

            MJpegOverHttpStreamer videoStreamer = new MJpegOverHttpStreamer(source);
            VideoEncodingParams encodingParams = new VideoEncodingParams
            {
                Width = destSize.Width, // options.Width,
                Height =  destSize.Height, // options.Height,
                FrameRate = options.FrameRate,
                EncoderName = "mjpeg",
            };
            
            var streamerTask = videoStreamer.Start(encodingParams, networkParams);
            */

            //AudioLoopbackSource audioStreamer = new AudioLoopbackSource();


            /*
            var audioParams = new AudioEncodingParams
            {
                SampleRate = 8000,
                Channels = 1,
                Encoding = "PCMU"
            };

            audioStreamer.Start(audioParams);
            */

            var uiThread = new Thread(() =>
            {
                Controls.StatisticForm statisticForm = new Controls.StatisticForm();
                statisticForm.Start();

                PreviewForm previewForm = new PreviewForm();
                previewForm.Setup(source);

                previewForm.Show();

                Application.Run();
            });
            uiThread.IsBackground = true;
            uiThread.SetApartmentState(ApartmentState.STA);


            Task.Run(() =>
            {
                Console.Title ="App running, press 'q' to quit...";


                while (Console.ReadKey().Key != ConsoleKey.Q) ;

                logger.Debug("'q' pressed...");
                //videoStreamer?.Close();

                //cancellationTokenSource.Cancel();

                videoStreamer?.Close();
                source?.Close();
            });

            Console.CancelKeyPress += (o, a) => 
            {
                logger.Debug("Ctrl+C pressed...");
                a.Cancel = true;

                //httpStreamer?.Close();
                //source?.Close();

                logger.Debug("Environment exit...");
                Environment.Exit(0);

    
            };

            Task.Run(() => 
            {        
                
                var ipaddr = IPAddress.Any;
                int port = 8888;

                logger.Info("Start input simulator...");


                Socket socket = null;
                try
                {
                    InputSimulator inputSimulator = new InputSimulator();


                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
                    socket.Bind(new IPEndPoint(ipaddr, port));

                    logger.Debug("Socket binded " + ipaddr.ToString() + " " + port);

                    byte[] buffer = new byte[1024];
                    while (true)
                    {
                        try
                        {
                            var bytesReceived = socket.Receive(buffer, SocketFlags.None);

                            if (bytesReceived > 0)
                            {
                                var message = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

                                var pars = message.Split(':');
                                if (pars != null && pars.Length > 1)
                                {
                                    var command = pars[0];
                                    if (command == "MouseMove")
                                    {
                                        logger.Debug(command + " " + string.Join(" ", pars));

                                        var position = pars[1];
                                        var pos = position.Split(' ');

                                        if (pos != null && pos.Length == 2)
                                        {
                                            var x = double.Parse(pos[0]);
                                            var y = double.Parse(pos[1]);
                                            inputSimulator.Mouse.MoveMouseTo(x, y);
                                        }
                                    }
                                    else if (command == "MouseUp")
                                    {
                                        var par = pars[1];
                                        var buttonParams = par.Split(' ');

                                        if (buttonParams != null && buttonParams.Length == 2)
                                        {
                                            logger.Debug(command + " " +  string.Join(" ", buttonParams));

                                            var button = (MouseButtons)int.Parse(buttonParams[0]);
                                            var click = int.Parse(buttonParams[1]);

                                            if (button == MouseButtons.Left)
                                            { 
                                                inputSimulator.Mouse.LeftButtonUp();
                                            }
                                            else if (button == MouseButtons.Right)
                                            {
                                                inputSimulator.Mouse.RightButtonUp();
                                            }
                                        }
                                    }
                                    else if (command == "MouseDown")
                                    {
                                        var par = pars[1];
                                        var buttonParams = par.Split(' ');

                                        if (buttonParams != null && buttonParams.Length == 2)
                                        {
                                            logger.Debug(command + " " + string.Join(" ", buttonParams));

                                            var button = (MouseButtons)int.Parse(buttonParams[0]);
                                            var click = int.Parse(buttonParams[1]);

                                            if (button == MouseButtons.Left)
                                            {
                                                inputSimulator.Mouse.LeftButtonDown();
                                            }
                                            else if (button == MouseButtons.Right)
                                            {
                                                inputSimulator.Mouse.RightButtonDown();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            
                        }
                    }
                }
                catch(Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    if (socket != null)
                    {
                        socket.Close();
                        socket = null;
                    }
                }

            });


            //Application.Run(previewForm);
            logger.Info("==========APP RUN ============");

            //var task =  TaskEx.WhenAllOrFirstException(new [] { streamerTask, captureTask });

            uiThread.Start();

            var task = Task.WhenAny(streamerTask, captureTask);

            var result = task.Result;
            var aex = result.Exception;
            if (aex != null)
            {
                logger.Error(aex);

                videoStreamer?.Close();
                source?.Close();
            }

            //var faultedTasks = result.Where(t => t.Exception != null);

            //foreach(var t in faultedTasks)
            //{
            //    var aex = t.Exception;
            //    if (aex != null)
            //    {
            //        logger.Error(aex);
            //    }
            //}



            //Task.WaitAll(streamerTask, captureTask);

            Utils.WinMM.timeEndPeriod(1);

            SharpDX.MediaFoundation.MediaManager.Shutdown();

            logger.Info("========== THE END ============");

            Console.Title = "App stopped, press any key to exit...";

            Console.ReadKey();


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
