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

        enum VendorId : uint
        {
            None = 0,
            Intel = 0x8086, //32902
            Nvidia = 0x10DE, //4318
            AMD = 0x1002,
            Microsoft = 0x1414,
            //...
            Unknown = 0xffff,
        }

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("MediaToolkitManager.Startup()");

            MediaToolkitManager.Startup();
            Console.WriteLine("========================RUN=======================");


             DisplayManager.Init();

            //NvApiTest.Run4();

            var gdiDevices = MediaToolkit.NativeAPIs.Utils.DisplayTool.EnumDisplayDevices();

            foreach (var adapter in gdiDevices.Keys)
            {
				Console.WriteLine("Adapter: ");

				Console.WriteLine(adapter.ToString());

                var monitors = gdiDevices[adapter];

				Console.WriteLine("Monitors: ");
				foreach (var m in monitors)
                {
                    Console.WriteLine(m.ToString());
                }
				Console.WriteLine("---------------------");
			}


            string path = AppDomain.CurrentDomain.BaseDirectory;
            string fileFullName = System.Reflection.Assembly.GetCallingAssembly().Location;
            //fileFullName = @"d:\Test.Encoder.exe";
            //fileFullName = @"vlc.exe";

           

            //string fileName = Path.GetFileName(fileFullName);
            //string name = Path.GetFileNameWithoutExtension(fileName);
            //bool forceIntegratedGPU = true;

            //var settings = NvApiTest.CreateShimRenderingSettings(forceIntegratedGPU);
            //NvApiTest.SetupNvProfile(name, fileName, settings);





            //NvApiTest.SetupNvOptimusProfile(name, fileName, forceIntegratedGPU);


            //var profileName = "Calculator";
            // profileName = "Skype Metro App";
            //NvApiTest.Run3(profileName);

            //DisplayDeviceTest.GetDisplayInfoTest();

            //NvApiTest.Run4();


            //MediaToolkit.Utils.RegistryTool.SetUserGpuPreferences("123423", 1);

            ////MediaToolkit.NativeAPIs.Utils.DisplayTool.DumpDevices();

            //var devices = MediaToolkit.NativeAPIs.Utils.DisplayTool.GetDisplayDevices();

            //foreach (var d in devices)
            //{
            //    Console.WriteLine(d .ToString());
            //    var monitors = d.Monitors;
            //    foreach (var m in monitors)
            //    {
            //        Console.WriteLine(m.ToString());
            //    }

            //    Console.WriteLine("------------------------------------");
            //    Console.WriteLine("");
            //}

           // Console.WriteLine(MediaToolkit.MediaFoundation.DxTool.LogDxInfo());

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
