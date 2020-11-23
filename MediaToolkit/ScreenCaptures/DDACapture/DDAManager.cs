using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using System.IO;

using System.Threading;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
//using SharpDX.Direct2D1;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

using GDI = System.Drawing;
using Direct2D = SharpDX.Direct2D1;
using MediaToolkit.Utils;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using MediaToolkit.Logging;
using MediaToolkit.SharedTypes;
using System.Windows.Forms;

namespace MediaToolkit.ScreenCaptures
{

    public class DDAOutputManager
    {
        private Dictionary<int, Dictionary<int, DDAOutput>> OutputDict = new Dictionary<int, Dictionary<int, DDAOutput>>();

        public DDAOutput GetOutput(int adapterIndex, int outputIndex)
        {
            Console.WriteLine("DDAOutputManager::GetOutput(...) " + adapterIndex + " " + outputIndex);
            DDAOutput output = null;
            if (OutputDict.ContainsKey(adapterIndex))
            {
                var outputs = OutputDict[adapterIndex];
                if (outputs.ContainsKey(outputIndex))
                {
                    Console.WriteLine("outputs[outputIndex]");

                    output = outputs[outputIndex];
                }
                else
                {
                    Console.WriteLine("new DDAOutput()");

                    output = new DDAOutput();
                    output.Init(adapterIndex, outputIndex);
                    outputs[outputIndex] = output;
                }
            }
            else
            {
                Console.WriteLine("new Dictionary<int, DDAOutput>()");

                var outputs = new Dictionary<int, DDAOutput>();
                output = new DDAOutput();
                output.Init(adapterIndex, outputIndex);
                outputs[outputIndex] = output;

                OutputDict[adapterIndex] = outputs;
            }

            return output;
        }

        public void Dispose()
        {

            Console.WriteLine("DDAOutputManager::Dispose()");

            foreach (var adapter in OutputDict.Keys)
            {
                var outputs = OutputDict[adapter];
                foreach (var output in outputs.Values)
                {
                    output.Close();
                }
            }

            OutputDict.Clear();
        }
    }


}
