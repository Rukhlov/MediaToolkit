using MediaToolkit.NativeAPIs.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.DeskMan
{
	class Program
	{
		static void Main(string[] args)
		{
			SystemEvents.SessionSwitch += (o, a) => 
			{
				Console.WriteLine("SystemEvents.SessionSwitch " + a.Reason);
			};

			SystemEvents.DisplaySettingsChanged += (o,a) => 
			{
				Console.WriteLine("SystemEvents.DisplaySettingsChanged");
			};

			SystemEvents.UserPreferenceChanged += (o, a) =>
			{
				Console.WriteLine("SystemEvents.UserPreferenceChanged " + a.Category);
			};

			var sessions = DesktopManager.GetActiveSessions();
			foreach(var s in sessions)
			{
				Console.WriteLine(string.Join(", " , s.ID, s.Name, s.Type, s.Username));
			}

			Task.Run(() => 
			{
				while (true)
				{

					bool result = DesktopManager.SwitchToInputDesktop();
					Console.WriteLine("SwitchToInputDesktop " + result);

					DesktopManager.GetCurrentDesktop(out string decktopName);
					Console.WriteLine("GetCurrentDesktop " + decktopName);

					Console.WriteLine("--------------------------------------");

					Thread.Sleep(1000);
				}


			});


			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
		}
	}
}
