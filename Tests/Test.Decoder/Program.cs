using MediaToolkit;
using MediaToolkit.NativeAPIs;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.Decoder
{
    static class Program
    {

        private static Logger logger = null;

        [STAThread]
        static void Main()
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Info("========== START ============");

			LogSystemInfo();

			try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                MediaToolkitManager.Startup();

				var graphicsMode = MediaToolkit.Utils.DisplayUtil.CheckGraphicsMode();
				logger.Info("GraphicsMode: " + graphicsMode);

				if(graphicsMode != MediaToolkit.Utils.GraphicsMode.Default && 
					graphicsMode != MediaToolkit.Utils.GraphicsMode.MultiGpu)
				{
					MessageBox.Show("Special graphics mode decected: " + graphicsMode);
				}

				Application.Run(new Form1());
            }
            catch(Exception ex)
            {
                logger.Fatal(ex);
            }
            finally
            {
                MediaToolkitManager.Shutdown();

                var dxLog = SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects();
                logger.Info("SharpDx Active Objects:\r\n------------------------------------------------------\r\n" + dxLog);

                logger.Info("========== THE END ============");
            }


            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

		private static void LogSystemInfo()
		{
			logger.Trace("LogSystemInfo()");

			//var sysInfo = "OS: "  + Environment.OSVersion + " " + (Environment.Is64BitOperatingSystem ? "x64" : "x86");
			logger.Info("OS: " + SystemInfo.GetOSInfo());
			logger.Info("CPU: " + SystemInfo.GetProcessorInfo());
			logger.Info("RAM: " + SystemInfo.GetMemoryInfo());


			using (var wi = System.Security.Principal.WindowsIdentity.GetCurrent())
			{
				var userName = wi.Name;
				var isElevated = (wi.Owner != wi.User);
				var isSystem = wi.IsSystem;

				if (isSystem)
				{
					logger.Info("Running as SYSTEM: " + isSystem);
				}

				if (isElevated)
				{
					logger.Info("Running as Admin: " + isElevated);
				}

				var IsRemoteSession = SystemInformation.TerminalServerSession;
				// rdp 
				if (IsRemoteSession)
				{
					logger.Info("Remotely Controlled: " + IsRemoteSession);
				}

				var IsCompositionEnabled = DwmApi.IsCompositionEnabled();
				if (!IsCompositionEnabled)
				{// выключена композитная отрисовка может быть только для Win7
					logger.Info("DWM: " + IsCompositionEnabled);
				}
			}


		}


		public class SystemInfo
		{
			public static string GetOSInfo()
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

					if (buildLab != "")
					{
						osInfo += " (" + buildLab + ")";
					}
				}


				return osInfo;
			}

			public static string GetProcessorInfo()
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

			public static string GetMemoryInfo()
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
		}
	}


}
