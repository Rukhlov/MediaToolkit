using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.Encoder
{
	class DisplaySettingChanged
	{
		public static void Run()
		{
			SystemEvents.SessionSwitch += (o, a) =>
			{
				Console.WriteLine("SystemEvents.SessionSwitch " + a.Reason);
			};

			SystemEvents.SessionEnded += (o, a) =>
			{
				Console.WriteLine("SystemEvents.SessionEnded " + a.Reason);
			};

			SystemEvents.DisplaySettingsChanging += (o, a) =>
			{
				Console.WriteLine("SystemEvents.DisplaySettingsChanging ");
			};

			SystemEvents.DisplaySettingsChanged += (o, a) =>
			{
				Console.WriteLine("SystemEvents.DisplaySettingsChanged ");
			};

			SystemEvents.UserPreferenceChanged += (o, a) =>
			{
				Console.WriteLine("SystemEvents.UserPreferenceChanged " + a.Category);
			};

			SystemEvents.PowerModeChanged += (o, a) =>
			{
				Console.WriteLine("SystemEvents.PowerModeChanged " + a.Mode);
			};

			SystemEvents.PaletteChanged += (o, a) =>
			{
				Console.WriteLine("SystemEvents.PaletteChanged " + a);
			};

			Console.WriteLine("Application.Run()");
			//Form f = new Form();
			Application.Run();

		}

	}
}
