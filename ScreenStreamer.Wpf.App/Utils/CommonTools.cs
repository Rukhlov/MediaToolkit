using System;
using System.Collections.Generic;
using System.Linq;
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
}
