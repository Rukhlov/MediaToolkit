using MediaToolkit.MediaFoundation;
using MediaToolkit.NativeAPIs;
using MediaToolkit.NativeAPIs.Utils;
using MediaToolkit.ScreenCaptures;
using Microsoft.Win32;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.DeskMan
{
	class Program
	{
		static void Main(string[] args)
		{

			//var applicationName = Path.Combine(Environment.SystemDirectory, "notepad.exe");

			//var res = DesktopManager.OpenInteractiveProcess(applicationName, "default", false, out AdvApi32.PROCESS_INFORMATION procInfo);
			//Console.WriteLine("OpenInteractiveProcess(...) " + res + " " + procInfo.dwProcessId);
			//Console.ReadKey();
			//return;


			if (args != null && args.Length > 0)
			{
				try
				{
					var applicationDir = Directory.GetCurrentDirectory();

					var applicatonFullName = Path.Combine(applicationDir, "Test.DeskMan.exe");

					//applicatonFullName = Path.Combine(@"C:\Windows", "regedit.exe");

					var pid = ProcessTool.StartProcessWithSystemToken(applicatonFullName, "");

					if (pid > 0)
					{
						var process = Process.GetProcessById(pid);
						if (process != null)
						{
							Console.WriteLine("New process started: " + process.ProcessName);
						}

					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);

				}

				Console.WriteLine("Press any key to exit");
				Console.ReadKey();

				//Process process = new Process();
				//var _args = new string[]
				//{
				//	"-s",
				//	"-i",
				//	"\"" + curFile + "\"",

				//	//"\""  + "\"" + curFile + "\"" + " -test" + "\"",

				//};
				//ProcessStartInfo pi = new ProcessStartInfo
				//{
				//	FileName = "PsExec.exe",
				//	Arguments = string.Join(" ", _args), //"-s -i Test.DeskMan.exe -test",
				//	Verb = "runas",
				//};

				//process.StartInfo = pi;

				//process.Start();


			}
			else
			{
				Task.Run(() => 
				{

					StartTest();
				});


				Console.WriteLine("Press any key to exit...");
				Console.ReadKey();
			}



		}


		private static void StartTest()
		{
			MediaToolkit.MediaToolkitManager.Startup();

			//var res = DesktopManager.OpenInteractiveProcess("Notepad.exe", "default", false, out AdvApi32.PROCESS_INFORMATION procInfo);
			//Console.WriteLine("OpenInteractiveProcess(...) " + res + " " +  procInfo.dwProcessId);
			//Console.ReadKey();

			SystemEvents.SessionSwitch += (o, a) =>
			{
				Console.WriteLine("SystemEvents.SessionSwitch " + a.Reason);
			};

			SystemEvents.DisplaySettingsChanged += (o, a) =>
			{
				Console.WriteLine("SystemEvents.DisplaySettingsChanged");
			};

			SystemEvents.UserPreferenceChanged += (o, a) =>
			{
				Console.WriteLine("SystemEvents.UserPreferenceChanged " + a.Category);
			};

			var sessions = DesktopManager.GetActiveSessions();
			foreach (var s in sessions)
			{
				Console.WriteLine(string.Join(", ", s.ID, s.Name, s.Type, s.Username));
			}


			var screen = Screen.PrimaryScreen;



			GDICapture capture = new GDICapture();
			capture.UseHwContext = false;

			var srcRect = screen.Bounds;
			var destSize = srcRect.Size;
			capture.Init(srcRect, destSize);


			var dir = @"D:\Temp\ScreenShots\" + DateTime.Now.ToString("HH_mm_ss_fff");
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			var _decktopName = "";

			while (true)
			{

				try
				{
					bool result = DesktopManager.SwitchToInputDesktop();

					if (!result)
					{
						Console.WriteLine("DesktopManager.SwitchToInputDesktop() " + result);
					}

					result = DesktopManager.GetCurrentDesktop(out string desktopName);
					if (!result)
					{
						Console.WriteLine("DesktopManager.GetCurrentDesktop() " + result);
					}

					if (desktopName != _decktopName)
					{
						_decktopName = desktopName;
						Console.WriteLine("Current desktop: " + _decktopName);

						Console.WriteLine("--------------------------------------");
					}

					result = capture.UpdateBuffer();

					if (result)
					{
						var buffer = capture.VideoBuffer;

						Bitmap bmp = buffer.bitmap;
						if (capture.UseHwContext)
						{
							var texture = capture.SharedTexture;

							Texture2D stagingTexture = null;
							try
							{
								var device = texture.Device;
								var descr = texture.Description;

								stagingTexture = new Texture2D(device,
									new Texture2DDescription
									{
										CpuAccessFlags = CpuAccessFlags.Read,
										BindFlags = BindFlags.None,
										Format = Format.B8G8R8A8_UNorm,
										Width = descr.Width,
										Height = descr.Height,
										MipLevels = 1,
										ArraySize = 1,
										SampleDescription = { Count = 1, Quality = 0 },
										Usage = ResourceUsage.Staging,
										OptionFlags = ResourceOptionFlags.None,
									});

								device.ImmediateContext.CopyResource(texture, stagingTexture);
								device.ImmediateContext.Flush();

								DxTool.TextureToBitmap(stagingTexture, ref bmp);

							}
							finally
							{
								stagingTexture?.Dispose();
							}


						}


						var timeNow = DateTime.Now.ToString("HH_mm_ss_fff");

						var fileName = _decktopName + " " + timeNow + ".jpg";

						var fullName = Path.Combine(dir, fileName);

						bmp.Save(fullName);
					}
					else
					{
						Console.WriteLine("capture.UpdateBuffer() " + result);
					}

					Thread.Sleep(1000);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);

				}

			}



			MediaToolkit.MediaToolkitManager.Shutdown();
		}
	}
}
