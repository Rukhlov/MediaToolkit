using MediaToolkit.Core;
using MediaToolkit.NativeAPIs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Utils
{

	public class DisplayInfo
	{
		public string FriendlyName { get; set; }
		public string Path { get; set; }
		public string GdiDeviceName { get; set; }
		public uint DisplayId { get; set; }

		public string AdapterDevicePath { get; set; }
		public Rectangle Bounds { get; set; }

		public uint PixelFormat { get; set; }
		public uint Rotation { get; set; }
		public uint Scaling { get; set; }

		public MediaRatio RefreshRate { get; set; }
	}


	public class DisplayDevice
	{
		public int EnumIndex { get; set; } = -1;
		public string DeviceName { get; set; } = "";
		public int VendorId { get; set; } = -1;
		public int DeviceId { get; set; } = -1;
		public string Description { get; set; } = "";

		public bool IsPrimary { get; set; } = false;
		public bool IsRemote { get; set; } = false;

		public override string ToString()
		{
			return "#" + EnumIndex + " " + string.Join("; ", DeviceName, Description, VendorId, DeviceId, IsPrimary, IsRemote);
		}
	}

	public enum GraphicsMode
	{
		Default,
		Hybrid,
		Remote,
		MultiGpu,

		Invalid,
	}

	public class DisplayUtil
	{

		public static GraphicsMode CheckGraphicsMode()
		{
			GraphicsMode adapterMode = GraphicsMode.Default;

			var gdiDevices = GetGdiDisplayDevices();
			var gdiDeivce0 = gdiDevices.FirstOrDefault();
			if (gdiDeivce0 == null)
			{
				return GraphicsMode.Invalid;
			}

			if (gdiDeivce0.IsRemote)
			{// RDP mode
				return GraphicsMode.Remote;
			}

			var dxDevices = GetDxDisplayDevices();

			foreach (var gdiDevice in gdiDevices)
			{
				// Console.WriteLine("GDI: " + gdiDevice);

				var deviceName = gdiDevice.DeviceName;

				var dxDevice = dxDevices.FirstOrDefault(d => d.DeviceName == deviceName);

				//Console.WriteLine("DiX: " + dxDevice);

				if (dxDevice.DeviceId != gdiDevice.DeviceId)
				{
					// проверяем NvOptimus, AMDSwitchableGraphics,...
					// сравниваем DeviceId полученные от GDI и DirectX для одного монитора
					// в гибридном режиме GDI определяет что монитры подключены к интегрированной карте(iGPU), а DirectX - к дискретной (dGPU)
					// непонятно насколько это правильно
					// но это единственный найденый способ проверки (в nvapi есть флаг NV_CHIPSET_INFO_HYBRID, но он obsolete и не работает)
					// TODO: в dxdiag.exe dGPU определяется как Render-Only Device, а iGPU -> FullDevice т.е где то в винде это можно найти...


					return GraphicsMode.Hybrid;
				}
			}

			if (dxDevices.Count > 1)
			{
				var dxDevice0 = dxDevices[0];
				if (dxDevices.All(d => d.EnumIndex == dxDevice0.EnumIndex))
				{// все мониторы подключены к одному адаптеру обычный режим
					adapterMode = GraphicsMode.Default;
				}
				else
				{// несколько активных видео адаптеров 
					adapterMode = GraphicsMode.MultiGpu;
				}
			}

			return adapterMode;

		}


        public static void SetUserGpuPreferences(string fileName, int UserGpuPreferences)
        {
            //Windows 10 Build 1809 and higher
            //Starting with Windows 10 build 19564, Microsoft updated the Graphics settings page (Settings > System > Display > Graphics settings),
            //allowing for better control over designating which GPU your apps run on.

            //х.з где это документировано найдено на форуме
            //https://social.msdn.microsoft.com/Forums/office/en-US/faaa3a92-ed9a-4878-82b9-a43e175cc6e4/graphics-performance-preference
            /*
             * HKEY_CURRENT_USER\SOFTWARE\Microsoft\DirectX\UserGpuPreferences
                Power savings:
                [application full path with \\ as path separators] = "GpuPreference=1;"
                Maximum performance:
                [application full path with \\ as path separators] = "GpuPreference=2;"
            */
            var name = @"Software\Microsoft\DirectX\UserGpuPreferences";
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(name, true))
            {
                if (key != null)
                {// if supported, use Windows 10 graphics performance settings
                    var value = "GpuPreference=" + UserGpuPreferences + ";";
                    key.SetValue(fileName, value);

                }
            }
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

			var gdiDisplayDevices = DisplayUtil.EnumDisplayDevices();
			int _adapterNum = 0;
			foreach (var adapter in gdiDisplayDevices.Keys)
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
					IsRemote = stateFlags.HasFlag(DisplayDeviceStateFlags.Remote),
					IsPrimary = stateFlags.HasFlag(DisplayDeviceStateFlags.PrimaryDevice),

				};

				displayDevices.Add(displayDevice);

				_adapterNum++;
			}

			return displayDevices;
		}


		public static List<DisplayInfo> GetDisplayInfos()
		{
			List<DisplayInfo> displayInfos = new List<DisplayInfo>();

			uint pathCount, modeCount;
			var result = User32.GetDisplayConfigBufferSizes(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);

			if (result != (int)HResult.S_OK)
			{
				throw new Win32Exception(result);
			}

			var displayPaths = new DISPLAYCONFIG_PATH_INFO[pathCount];
			var displayModes = new DISPLAYCONFIG_MODE_INFO[modeCount];

			result = User32.QueryDisplayConfig(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
				ref pathCount, displayPaths, ref modeCount, displayModes, IntPtr.Zero);

			if (result != (int)HResult.S_OK)
			{
				throw new Win32Exception(result);
			}


			Dictionary<uint, DISPLAYCONFIG_PATH_SOURCE_INFO> sourceInfos = new Dictionary<uint, DISPLAYCONFIG_PATH_SOURCE_INFO>();
			Dictionary<uint, DISPLAYCONFIG_PATH_TARGET_INFO> targetInfos = new Dictionary<uint, DISPLAYCONFIG_PATH_TARGET_INFO>();

			for (int i = 0; i < pathCount; i++)
			{
				var pathInfo = displayPaths[i];
				
				var sourceInfo = pathInfo.sourceInfo;
				var srcModeIdx = sourceInfo.modeInfoIdx;
				sourceInfos[srcModeIdx] = sourceInfo;

				var targetInfo = pathInfo.targetInfo;
				var targetModeIdx = targetInfo.modeInfoIdx;
				targetInfos[targetModeIdx] = targetInfo;
			}

		    for (uint i = 0; i < modeCount; i += 2)
			{
				var targetDisplayMode = displayModes[i];
				var sourceDisplayMode = displayModes[i + 1];

				var targetPathInfo = targetInfos[i];
				var sourcePathInfo = sourceInfos[i + 1];
				
				if (targetDisplayMode.infoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
				{
					var monitorInfo = GetDisplayConfigTargetDeviceName(targetDisplayMode.adapterId, targetDisplayMode.id);
					var adapterInfo = GetDisplayConfigAdapterName(targetDisplayMode.adapterId, targetDisplayMode.id);

					var gdiDeviceInfo = GetDisplayConfigSourceDeviceName(sourceDisplayMode.adapterId, sourceDisplayMode.id);
					var sourceMode = sourceDisplayMode.modeInfo.sourceMode;

					var pos = sourceMode.position;
					int width = (int)sourceMode.width;
					int height = (int)sourceMode.height;
					var refreshRate = targetPathInfo.refreshRate;

					var di = new DisplayInfo
					{
						FriendlyName = monitorInfo.monitorFriendlyDeviceName,
						Path = monitorInfo.monitorDevicePath,
						DisplayId = targetDisplayMode.id,
						GdiDeviceName = gdiDeviceInfo.viewGdiDeviceName,
						AdapterDevicePath = adapterInfo.adapterDevicePath,
						Bounds = new Rectangle(pos.x, pos.y, width, height),
						PixelFormat = (uint)sourceMode.pixelFormat,
						Rotation = (uint)targetPathInfo.rotation,
						Scaling = (uint)targetPathInfo.scaling,
						RefreshRate = new MediaRatio((int)refreshRate.Numerator, (int)refreshRate.Denominator),

					};

					displayInfos.Add(di);
				}
				else
				{// unexpected ...

				}
			}

			return displayInfos;
		}



		public static DISPLAYCONFIG_SOURCE_DEVICE_NAME GetDisplayConfigSourceDeviceName(LUID adapterId, uint sourceId)
		{
			DISPLAYCONFIG_SOURCE_DEVICE_NAME deviceInfo = new DISPLAYCONFIG_SOURCE_DEVICE_NAME
			{
				size = (uint)Marshal.SizeOf(typeof(DISPLAYCONFIG_SOURCE_DEVICE_NAME)),
				adapterId = adapterId,
				id = sourceId,
				type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME

			};

			int result = User32.DisplayConfigGetDeviceInfo(ref deviceInfo);
			if (result != (int)HResult.S_OK)
			{
				throw new Win32Exception(result);
			}

			return deviceInfo;
		}


		public static DISPLAYCONFIG_TARGET_DEVICE_NAME GetDisplayConfigTargetDeviceName(LUID adapterId, uint targetId)
		{

			var deviceName = new DISPLAYCONFIG_TARGET_DEVICE_NAME
			{
				size = (uint)Marshal.SizeOf(typeof(DISPLAYCONFIG_TARGET_DEVICE_NAME)),
				adapterId = adapterId,
				id = targetId,
				type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME

			};

			var result = User32.DisplayConfigGetDeviceInfo(ref deviceName);
			if (result != (int)HResult.S_OK)
			{
				throw new Win32Exception(result);
			}

			return deviceName;
		}

		public static DISPLAYCONFIG_ADAPTER_NAME GetDisplayConfigAdapterName(LUID adapterId, uint targetId)
		{
			var adapterName = new DISPLAYCONFIG_ADAPTER_NAME
			{
				size = (uint)Marshal.SizeOf(typeof(DISPLAYCONFIG_ADAPTER_NAME)),
				adapterId = adapterId,
				id = targetId,
				type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADAPTER_NAME

			};

			var result = User32.DisplayConfigGetDeviceInfo(ref adapterName);
			if (result != (int)HResult.S_OK)
			{
				throw new Win32Exception(result);
			}

			return adapterName;
		}



		public static Dictionary<DISPLAY_DEVICE, IEnumerable<DISPLAY_DEVICE>> EnumDisplayDevices(bool attached = true)
		{
			Dictionary<DISPLAY_DEVICE, IEnumerable<DISPLAY_DEVICE>> displayDict = new Dictionary<DISPLAY_DEVICE, IEnumerable<DISPLAY_DEVICE>>();

			try
			{
				DISPLAY_DEVICE adapter = new DISPLAY_DEVICE
				{
					cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE)),
				};

				uint adapterNum = 0;
				while (User32.EnumDisplayDevices(null, adapterNum, ref adapter, 0))
				{
					if (attached)
					{
						if (!adapter.StateFlags.HasFlag(DisplayDeviceStateFlags.AttachedToDesktop))
						{
							adapterNum++;
							continue;
						}
					}

					DISPLAY_DEVICE monitor = new DISPLAY_DEVICE
					{
						cb = Marshal.SizeOf(typeof(DISPLAY_DEVICE)),
					};

					uint monitorNum = 0;
					var adapterName = adapter.DeviceName;
					List<DISPLAY_DEVICE> monitors = new List<DISPLAY_DEVICE>();

					while (User32.EnumDisplayDevices(adapterName, monitorNum, ref monitor, 0))
					{
						monitors.Add(monitor);
						monitorNum++;
					}

					displayDict.Add(adapter, monitors);

					adapterNum++;

				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}

			return displayDict;
		}

	}

}
