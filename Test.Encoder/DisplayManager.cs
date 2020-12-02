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

		public bool IsRemote { get; set; } = false;
		public bool IsGdiDevice { get; set; } = false;

        public override string ToString()
        {
            return "#" + EnumIndex + " " + string.Join("; ", DeviceName, Description, VendorId, DeviceId, IsGdiDevice);
        }

    }
    class DisplayManager
    {

        public static void Init()
        {
            Console.WriteLine("GetGdiDisplayDevices()");
            var gdiDevices = GetGdiDisplayDevices();

            Console.WriteLine("GetDxDisplayDevices()");
            var dxDevices = GetDxDisplayDevices();

            bool hybridModeEnabled = false;
			bool remoteMode = false;
            foreach (var device in gdiDevices)
            {
                Console.WriteLine("GDI: " + device);

                var deviceName = device.DeviceName;
				remoteMode = device.IsRemote;

                var dxDevice = dxDevices.FirstOrDefault(d => d.DeviceName == deviceName);

                Console.WriteLine("DiX: " + dxDevice);

				if (!remoteMode)
				{
					if (dxDevice.VendorId != device.VendorId)
					{
						hybridModeEnabled = true;
						break;
					}
				}

            }

            Console.WriteLine("HybridModeEnabled = " + hybridModeEnabled);
			Console.WriteLine("RemoteAdapterMode = " + remoteMode);


			Console.WriteLine("---------------------");

            //Console.WriteLine("GetDxDisplayDevices()");

            //foreach (var device in dxDevices)
            //{
            //    Console.WriteLine(device);

            //}




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
							
                        };

                        displayDevices.Add(displayDevice);
                    }

                    foreach(var o in outputs)
                    {
                        if(o!=null && !o.IsDisposed)
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

			var _displayDevices = DisplayTool.EnumDisplayDevices();
			int _adapterNum = 0;
			foreach (var adapter in _displayDevices.Keys)
			{
				var deviceName = adapter.DeviceName;
				var pciInfo = PciDeviceInfo.Parse(adapter.DeviceID);

				var stateFlags = adapter.StateFlags;
				DisplayDevice displayDevice = new DisplayDevice
				{
					EnumIndex = (int)_adapterNum,
					DeviceName = deviceName,
					Description = adapter.DeviceString,
					VendorId = pciInfo.VendorId,
					DeviceId = pciInfo.DeviceId,
					IsGdiDevice = true,
					IsRemote = stateFlags.HasFlag(DisplayDeviceStateFlags.Remote),

				};

				displayDevices.Add(displayDevice);

				_adapterNum++;
			}



            //DISPLAY_DEVICE dd = new DISPLAY_DEVICE
            //{
            //    cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE)),
            //};

            //try
            //{
            //    uint adapterNum = 0;
            //    while (User32.EnumDisplayDevices(null, adapterNum, ref dd, 0))
            //    {
            //        if (attached)
            //        {
            //            if (!dd.StateFlags.HasFlag(DisplayDeviceStateFlags.AttachedToDesktop))
            //            {
            //                adapterNum++;
            //                continue;
            //            }
            //        }

            //        uint monitorNum = 0;
            //        var deviceName = dd.DeviceName;
            //        var pciInfo = PciDeviceInfo.Parse(dd.DeviceID);

            //        DisplayDevice displayDevice = new DisplayDevice
            //        {
            //            EnumIndex = (int)adapterNum,
            //            DeviceName = deviceName,
            //            Description = dd.DeviceString,
            //            VendorId = pciInfo.VendorId,
            //            DeviceId = pciInfo.DeviceId,
            //            IsGdiDevice = true,
            //        };

            //        displayDevices.Add(displayDevice);

            //        DISPLAY_DEVICE md = new DISPLAY_DEVICE
            //        {
            //            cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE)),
            //        };

            //        while (User32.EnumDisplayDevices(deviceName, monitorNum, ref md, 0))
            //        {
            //            monitorNum++;
            //        }


            //        adapterNum++;

            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}

            return displayDevices;
        }


    }
}
