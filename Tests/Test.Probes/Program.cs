﻿using System;
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
using MediaToolkit.Utils;
using Test.Probe.Video;
using Test.Probe;
using Test.Probe.NvApi;

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

			//FFmpegDecoderTest.Run();
			//var info = GpuHardwareInfo.GetHardwareInfos();
			//foreach (var i in info)
			//{
			//    Console.WriteLine(i.AdapterString + " " + StringHelper.SizeSuffix(i.MemorySize, 0));

			//}

			//EnumWindowsTest.Run();

			//FullScreenSwitchTest.Run();

			//CopyImageTest.Run();


			//GDIRendererTest.Run();
			//SimpleSwapChain.Run();

			//IDRDecoder.Run();

			NvEncTest.Run();
			//NvDecTest.Run();
			//VideoDecoderTest.Run();
			//Probe.Utils.WindowHookTest.Run();

			//var encoderDescrs = FFmpegLib.Utils.GetH264Encoders();

			//foreach(var d in encoderDescrs)
			//{
			//    Console.WriteLine(d.ToString() + " (" + d.Description + ")");
			//}


			//Console.WriteLine( MediaToolkit.DirectX.DxTool.LogDxInfo());

			//DatapathCaptTest.Run();
			//RgbHelperTest.Run();

			//D3D11ConvertersTest.Run();

			////MftH264Test.Run();
			//int count = 10;
			//while (count-->0)
			//{

			//    DxTextureTest.Run2();
			//    Thread.Sleep(100);
			//}


			//var displayInfos = DisplayUtil.GetDisplayConfigInfos();

			// FFmpegPixConverterTest.Run();

			//DisplaySettingChanged.Run();
			//SimpleSwapChain.Run();

			//RgbToNv12Converter.Run();

			//FFmpegJpegEncoderTest.Run();

			//D3D11VideoProcessor videoProcessor = new D3D11VideoProcessor();
			//videoProcessor.Test();



			//var adapterMode = DisplayUtil.CheckGraphicsMode();

			//         Console.WriteLine(">>>>>>>>>>> Graphics System Mode: " + adapterMode);
			//         Console.WriteLine("-----------------------------\r\n");
			//         //NvApiTest.Run4();

			//         var dxDevices = DisplayUtil.GetDxDisplayDevices();
			//         foreach(var d in dxDevices)
			//         {
			//             Console.WriteLine(d.ToString());
			//         }


			//         var gdiDevices = DisplayUtil.EnumDisplayDevices(false);

			//         foreach (var adapter in gdiDevices.Keys)
			//         {
			//	Console.WriteLine("Adapter: ");

			//	Console.WriteLine(adapter.ToString());

			//             var monitors = gdiDevices[adapter];

			//	Console.WriteLine("Monitors: ");
			//	foreach (var m in monitors)
			//             {
			//                 Console.WriteLine(m.ToString());
			//             }
			//	Console.WriteLine("---------------------");
			//}


			//         string path = AppDomain.CurrentDomain.BaseDirectory;
			//         string fileFullName = System.Reflection.Assembly.GetCallingAssembly().Location;
			//         //fileFullName = @"d:\Test.Encoder.exe";
			//         //fileFullName = @"vlc.exe";



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
