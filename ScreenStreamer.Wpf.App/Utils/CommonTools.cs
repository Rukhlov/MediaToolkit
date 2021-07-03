using MediaToolkit.NativeAPIs;
using MediaToolkit.Utils;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScreenStreamer.Wpf.Utils
{
	public class ConfigTools
	{
		public static bool TryGetAppSettingsValue<T>(string name, out T t)
		{
			bool Result = false;
			t = default(T);
			try
			{
				var appSettings = System.Configuration.ConfigurationManager.AppSettings;
				if (appSettings != null)
				{
					Result = TryGetValueFromCollection(appSettings, name, out t);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}


			return Result;
		}

		public static bool TryGetValueFromCollection<T>(System.Collections.Specialized.NameValueCollection settings, string paramName, out T t)
		{
			Console.WriteLine("TryGetValueFromCollection(...) " + paramName);
			bool Result = false;

			t = default(T);
			if (settings == null)
			{
				Console.WriteLine("TryGetParams(...) settings == null");

				return Result;
			}

			if (string.IsNullOrEmpty(paramName))
			{
				Console.WriteLine("TryGetParams(...) paramName == null");

				return Result;
			}

			if (settings.Count <= 0)
			{

				Console.WriteLine("TryGetParams(...) settings.Count <= 0");

				return Result;

			}

			try
			{
				var val = settings[paramName];
				if (val != null)
				{
					val = val.Trim();
				}

				if (!string.IsNullOrEmpty(val))
				{
					Console.WriteLine(paramName + " = " + val);

					var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
					if (converter != null)
					{
						t = (T)converter.ConvertFromString(val);
						Result = true;
					}
				}
				else
				{
					Console.WriteLine(paramName + " not found");
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			return Result;
		}

	}

	public class SystemInfo
	{
        public static string LogOSInfo()
        {
            Microsoft.Win32.RegistryKey localMachineKey = Microsoft.Win32.Registry.LocalMachine;
            if (Environment.Is64BitOperatingSystem)
            { // в wow6432 почему то вместо Pro версии - Enterprise!
                localMachineKey = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine,
                    Microsoft.Win32.RegistryView.Registry64);
            }
            string osInfo = "";
            var regKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            Microsoft.Win32.RegistryKey key = localMachineKey.OpenSubKey(regKey);
            if (key != null)
            {
                var productName = key.GetValue("ProductName")?.ToString() ?? "";
                var csdVersion = key.GetValue("CSDVersion")?.ToString() ?? "";
                var buildLab = key.GetValue("BuildLab")?.ToString() ?? "";
                if (productName != "")
                {
                    osInfo = (productName.StartsWith("Microsoft") ? "" : "Microsoft ") + productName;
                }

                if (csdVersion != "")
                {
                    osInfo += " " + csdVersion;
                }

                if(buildLab!= "")
                {
                    osInfo += " (" + buildLab + ")";
                }
            }


            return osInfo;
        }

        public static string LogProcessorInfo()
		{
			string processInfo = "";
			var regKey = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0";
			Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regKey);
			if (key != null)
			{
				processInfo = key.GetValue("ProcessorNameString")?.ToString() ?? "";
				string procSpeed = key.GetValue("~MHz")?.ToString() ?? "";
				if (!string.IsNullOrEmpty(procSpeed))
				{
					processInfo += " " + procSpeed + "MHz";
				}
			}
			return processInfo;
		}

		public static string LogMemoryInfo()
		{
			string memoryInfo = "";
			//if(Kernel32.GetPhysicallyInstalledSystemMemory(out var totalMemory))
			//{
			//	memoryInfo = MediaToolkit.Utils.StringHelper.SizeSuffix(totalMemory * 1024);
			//}
			Kernel32.MEMORYSTATUSEX memoryStatus = new Kernel32.MEMORYSTATUSEX();
			if (Kernel32.GlobalMemoryStatusEx(memoryStatus))
			{
				var totalMemory = memoryStatus.ullTotalPhys / 1024 / 1024 + "MB";
				var freeMemory = memoryStatus.ullAvailPhys / 1024 / 1024 + "MB";
				memoryInfo = totalMemory + " / " + freeMemory + " free";
			}
			return memoryInfo;
		}

        public static string LogGpuInfo()
        {
            var sb = new StringBuilder();
            var gpuHardwareInfos = GpuHardwareInfo.GetHardwareInfos().ToArray();
             
            for(int i=0;i<gpuHardwareInfos.Length; i++)
            {
                var info = gpuHardwareInfos[i];

                sb.AppendLine("---------------------------");
                sb.AppendLine("GPU #" + i);
                sb.AppendLine(info.AdapterString);
                sb.AppendLine(info.BiosString);
                sb.AppendLine(info.ChipType);
                sb.AppendLine(info.DacType);
                sb.AppendLine(info.DriverDate);
                sb.AppendLine(info.DriverVersion);

                sb.AppendLine(StringHelper.SizeSuffix(info.MemorySize));

                var driverDlls = info.DriverDlls;
                if(driverDlls!=null && driverDlls.Length > 0)
                {
                    var fileNames = new List<string>();
                    foreach (var f in driverDlls)
                    {
                        fileNames.Add(Path.GetFileName(f));
                    }
                    sb.AppendLine(string.Join(";", fileNames));
                }
            }
            
            return sb.ToString();
        }
	}

}
