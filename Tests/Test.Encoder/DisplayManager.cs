using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MediaToolkit.NativeAPIs;
using MediaToolkit.NativeAPIs.Utils;
using MediaToolkit.Utils;

namespace Test.Encoder
{

    //public class GpuDevice
    //{
    //    public int EnumIndex { get; set; } = -1;
    //    public int VendorId { get; set; } = -1;
    //    public int DeviceId { get; set; } = -1;
    //    public string DeviceName { get; set; } = "";

    //    public string Description { get; set; } = "";

    //    public int Flags { get; set; } = -1;

    //    public bool IsGdiDevice { get; set; } = false;

    //    public List<DisplayDevice> Displays { get; set; } = new List<DisplayDevice>();


    //    public override string ToString()
    //    {
    //        return "#" + EnumIndex + " " + string.Join("; ", DeviceName, VendorId, DeviceId, Description, Flags, IsGdiDevice);
    //    }
    //}

    //public class DisplayDevice
    //{
    //    public int EnumIndex { get; set; } = -1;
    //    public string DeviceName { get; set; } = "";
    //    public int VendorId { get; set; } = -1;
    //    public int DeviceId { get; set; } = -1;
    //    public string Description { get; set; } = "";


    //    public Rectangle Bounds { get; set; }
    //    public int Flags { get; set; } = -1;

    //    public override string ToString()
    //    {
    //        return "#" + EnumIndex + " " + string.Join("; ", DeviceName, VendorId, DeviceId, Description, Flags);
    //    }
    //}



    public class DisplayDevice
    {
        public int EnumIndex { get; set; } = -1;
        public string DeviceName { get; set; } = "";
        public int VendorId { get; set; } = -1;
        public int DeviceId { get; set; } = -1;
        public string Description { get; set; } = "";

        public bool IsPrimary { get; set; } = false;
        public bool IsRemote { get; set; } = false;

        public bool IsGdiDevice { get; set; } = false;

        public override string ToString()
        {
            return "#" + EnumIndex + " " + string.Join("; ", DeviceName, Description, VendorId, DeviceId, IsPrimary, IsRemote);
        }
    }

    public enum DisplayAdapterMode
    {
        DefaultMode,
        HybridModeEnabled,
        RemoteDisplayAdapter,
        MultiGpuMode,
        //...

        Invalid,
    }

    public class DisplayManager
    {

        public static DisplayAdapterMode CheckDisplayAdapterMode()
        {
            DisplayAdapterMode adapterMode = DisplayAdapterMode.DefaultMode;

            var gdiDevices = GetGdiDisplayDevices();
            var gdiDeivce0 = gdiDevices.FirstOrDefault();
            if(gdiDeivce0 == null)
            {
                return DisplayAdapterMode.Invalid;
            }

            if (gdiDeivce0.IsRemote)
            {// RDP mode
                return DisplayAdapterMode.RemoteDisplayAdapter;
            }

            var dxDevices = GetDxDisplayDevices();

            foreach (var gdiDevice in gdiDevices)
            {
                Console.WriteLine("GDI: " + gdiDevice);

                var deviceName = gdiDevice.DeviceName;

                var dxDevice = dxDevices.FirstOrDefault(d => d.DeviceName == deviceName);

                Console.WriteLine("DiX: " + dxDevice);

                if (dxDevice.DeviceId != gdiDevice.DeviceId)
                {// разные GDI и DirectX адаптеры  
                 // гибридный режим NvOptimus, AMDSwitchableGraphics,...
                 // TODO: насколько это правильно, но других способов не найдено ?!
                    return DisplayAdapterMode.HybridModeEnabled;
                }
            }

            if (dxDevices.Count > 1)
            {
                var dxDevice0 = dxDevices[0];
                if (dxDevices.All(d => d.EnumIndex == dxDevice0.EnumIndex))
                {// все мониторы подключены к одному адаптеру обычный режим
                    adapterMode = DisplayAdapterMode.DefaultMode;
                }
                else
                {// несколько видео адаптеров 
                    adapterMode = DisplayAdapterMode.MultiGpuMode;
                }
            }

            return adapterMode;

        }

        public static List<DisplayDevice> GetDxDisplayDevices(bool attached = true)
        {
            List<DisplayDevice> displayDevices = new List<DisplayDevice>();


            using (var dxgiFactory = new SharpDX.DXGI.Factory1())
            {
                var adapters = dxgiFactory.Adapters1;

                for (int adapterIndex = 0; adapterIndex < adapters.Length; adapterIndex++)
                {
                    var adapter = adapters[adapterIndex];
                    var adaptDescr = adapter.Description1;

                    var outputs = adapter.Outputs;

                    for (int outputIndex = 0; outputIndex < outputs.Length; outputIndex++)
                    {
                        var output = outputs[outputIndex];

                        var outputDescr = output.Description;
                        if (attached)
                        {
                            if (!outputDescr.IsAttachedToDesktop)
                            {
                                continue;
                            }
                        }

                        var flags = adaptDescr.Flags;
                        DisplayDevice displayDevice = new DisplayDevice
                        {
                            EnumIndex = adapterIndex,
                            DeviceName = outputDescr.DeviceName,
                            Description = adaptDescr.Description,
                            VendorId = adaptDescr.VendorId,
                            DeviceId = adaptDescr.DeviceId,
                            IsGdiDevice = false,
                            IsRemote = flags.HasFlag(SharpDX.DXGI.AdapterFlags.Remote),
                            IsPrimary = (adapterIndex == 0 && outputIndex == 0),

                        };

                        displayDevices.Add(displayDevice);
                    }

                    foreach (var o in outputs)
                    {
                        if (o != null && !o.IsDisposed)
                        {
                            o.Dispose();
                        }
                    }
                }

                foreach (var a in adapters)
                {
                    if (a != null && !a.IsDisposed)
                    {
                        a.Dispose();
                    }
                }

            }


            return displayDevices;
        }

        public static List<DisplayDevice> GetGdiDisplayDevices(bool attached = true)
        {
            List<DisplayDevice> displayDevices = new List<DisplayDevice>();

            var _displayDevices = DisplayUtil.EnumDisplayDevices();
            int _adapterNum = 0;
            foreach (var adapter in _displayDevices.Keys)
            {
                var deviceName = adapter.DeviceName;
                var pciInfo = PciDeviceInfo.Parse(adapter.DeviceID);

                var stateFlags = adapter.StateFlags;
                DisplayDevice displayDevice = new DisplayDevice
                {
                    EnumIndex = _adapterNum,
                    DeviceName = deviceName,
                    Description = adapter.DeviceString,
                    VendorId = pciInfo.VendorId,
                    DeviceId = pciInfo.DeviceId,
                    IsGdiDevice = true,
                    IsRemote = stateFlags.HasFlag(DisplayDeviceStateFlags.Remote),
                    IsPrimary = stateFlags.HasFlag(DisplayDeviceStateFlags.PrimaryDevice),

                };

                displayDevices.Add(displayDevice);

                _adapterNum++;
            }

            return displayDevices;
        }


    }
}
