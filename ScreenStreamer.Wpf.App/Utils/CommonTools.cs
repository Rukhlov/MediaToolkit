using MediaToolkit.NativeAPIs;
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


    static class WinConsole
    {
        static public void Initialize(bool alwaysCreateNewConsole = true)
        {
            const uint ATTACH_PARRENT = 0xFFFFFFFF;

            bool consoleAttached = true;
            if (alwaysCreateNewConsole || (!Kernel32.AttachConsole(ATTACH_PARRENT) && Marshal.GetLastWin32Error() != Win32ErrorCodes.ERROR_ACCESS_DENIED))
            {
                consoleAttached = Kernel32.AllocConsole();
            }

            if (consoleAttached)
            {
                InitializeOutStream();
                InitializeInStream();
            }
        }

        public static void AllocConsole()
        {
            var hWnd = Kernel32.GetConsoleWindow();
            if (hWnd == IntPtr.Zero)
            {
                bool result = Kernel32.AllocConsole();
                if (result)
                {
                    InitializeOutStream();
                    InitializeInStream();
                }
            }
        }

        private static void InitializeOutStream()
        {
            var fileHandle = Kernel32.CreateConOutSafeHandle();
            if (!fileHandle.IsInvalid)
            {
                var fs = new FileStream(fileHandle, FileAccess.Write);
                if (fs != null)
                {
                    var writer = new StreamWriter(fs, Encoding.Default) { AutoFlush = true };
                    Console.OutputEncoding = writer.Encoding;
                    //var writer = new StreamWriter(fs, Console.OutputEncoding) { AutoFlush = true };
                    Console.SetOut(writer);
                    Console.SetError(writer);
                }
            }

        }

        private static void InitializeInStream()
        {
            var fileHandle = Kernel32.CreateConInSafeHandle();
            if (!fileHandle.IsInvalid)
            {
                var fs = new FileStream(fileHandle, FileAccess.Read);
                if(fs != null)
                {
                    var reader = new StreamReader(fs, Encoding.Default);
                    Console.InputEncoding = reader.CurrentEncoding;
                    Console.SetIn(reader);
                }
            }
        }

    }
}
