using MediaToolkit.Nvidia.NvAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvApiTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========================RUN=======================");
            Console.WriteLine("Is64BitProcess: " + Environment.Is64BitProcess);

            NvApiTest.Run();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }


    class NvApiTest
    {
        public static void Run()
        {
            Console.WriteLine("NvApiTest::Run() BEGIN");
            var nvapi = new NvAPI();

            var res = nvapi.Initialize();

            Console.WriteLine("NvAPI_Initialize() " + res);

            var status = nvapi.DRS_CreateSession(out var phSession);


            Console.WriteLine("NvAPI_DRS_CreateSession() " + status + " " + phSession);


            status = nvapi.DRS_DestroySession(phSession);

            Console.WriteLine("NvAPI_DRS_DestroySession() " + status + " " + phSession);

            res = nvapi.Unload();

            Console.WriteLine("NvAPI_Unload() " + res);

            Console.WriteLine("NvApiTest::Run() END");
        }
    }
}
