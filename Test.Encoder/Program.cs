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

            Test.Encoder.DDATest.DDATest.Run();

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
