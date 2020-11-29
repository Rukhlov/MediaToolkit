using System;
using MediaToolkit;


using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Diagnostics;

using System.Windows.Forms;

namespace Test.Encoder
{
    public partial class  Program
    {

 
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("MediaToolkitManager.Startup()");

            MediaToolkitManager.Startup();
            Console.WriteLine("========================RUN=======================");

			//NvApiTest.SetupNvOptimusProfile("TEST1", "TEST1.exe", true);

			//NvApiTest.GetDisplayInfoTest();

			NvApiTest.Run2();


            //MediaToolkit.Utils.RegistryTool.SetUserGpuPreferences("123423", 1);

            ////MediaToolkit.NativeAPIs.Utils.DisplayTool.DumpDevices();

            //var devices = MediaToolkit.NativeAPIs.Utils.DisplayTool.GetDisplayDevices();

            //foreach (var d in devices)
            //{
            //    Console.WriteLine(d.ToString());
            //    var monitors = d.Monitors;
            //    foreach (var m in monitors)
            //    {
            //        Console.WriteLine(m.ToString());
            //    }

            //    Console.WriteLine("------------------------------------");
            //    Console.WriteLine("");
            //}

            //Console.WriteLine(MediaToolkit.MediaFoundation.DxTool.LogDxInfo());

            // Test.Encoder.DDATest.DDATest.Run();

            //MfTransformTests.FindEncoderTest();

            //SimpleSwapChain.Run();

            // WicTest1.Run();

            // CopyAcrossGPUTest.Run();

            //NewMethod1();

            //Console.WriteLine(DxTool.LogDxInfo());


            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());

            Console.WriteLine("MediaToolkitManager.Shutdown()");
            MediaToolkitManager.Shutdown();


            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();


        }




    }




}
