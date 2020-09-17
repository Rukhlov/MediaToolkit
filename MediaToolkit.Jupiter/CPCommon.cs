using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Jupiter
{
    public class CPException : Exception
    {
        public CPException(ResultCodes result) : this(result, GetDescriptionFromResultCode(result))
        { }

        public CPException(string descr):this(ResultCodes.S_FALSE, descr)
        { }

        public CPException(ResultCodes result, string descr)
        {
            this.Result = result;
            this.Description = descr;
        }

        public ResultCodes Result { get; private set; } = ResultCodes.S_OK;
        public string Description { get; private set; } = "";

        public override string Message => Description + " " + "0x" + Result.ToString("X");

        public static string GetDescriptionFromResultCode(ResultCodes ResultCode)
        {
            string descr = "";
            if (ResultCode == ResultCodes.E_INVALID_WINID)
            {
                descr = "Invalid window ID";
            }
            else if (ResultCode == ResultCodes.E_NOTFOUND)
            {
                descr = "Not found";
            }
            else if (ResultCode == ResultCodes.E_WINTYPEMISMATCH)
            {
                descr = "Window type mismatch";
            }
            else if (ResultCode == ResultCodes.E_INVALID_ARGS)
            {
                descr = "Invalid argument";
            }
            else if (ResultCode == ResultCodes.E_INVALID_ARCHIVE_VERSION)
            {
                descr = "Invalid archive version";
            }
            else if (ResultCode == ResultCodes.E_ARCHIVE_NOTFOUND)
            {
                descr = "Archive not found";
            }
            else if (ResultCode == ResultCodes.E_WINID_ALLREADY_USED)
            {
                descr = "Window ID already used";
            }
            else if (ResultCode == ResultCodes.E_INVALID_FORMAT)
            {
                descr = "Invalid archive format";
            }
            else if (ResultCode == ResultCodes.E_FILE_NOTEXIST)
            {
                descr = "The specified file does not exist";
            }
            else if (ResultCode == ResultCodes.E_WIN_CANNOT_SHOW_OR_REMOVE)
            {
                descr = "Cannot show or remove the window in the way specified";
            }
            else if (ResultCode == ResultCodes.E_PARS_NOT_ENOUGH)
            {
                descr = "Not enough parameters supplied";
            }
            else if (ResultCode == ResultCodes.E_TOMANY_PARS_SUPPLIED)
            {
                descr = "Too may parameters supplied";
            }
            else if (ResultCode == ResultCodes.E_INVALID_METHODNAME)
            {
                descr = "Invalid RMC method name";
            }
            else if (ResultCode == ResultCodes.E_INVALID_OBJECTNAME)
            {
                descr = "Invalid RMC object name";
            }
            else if (ResultCode == ResultCodes.E_BAD_FORMAT)
            {
                descr = "Bad parameter format";
            }
            else if (ResultCode == ResultCodes.E_UNSUPPORTED_DISPLAY_FORMAT)
            {
                descr = "Unsupported display format";
            }
            else if (ResultCode == ResultCodes.E_NO_TIMING_SELECTED)
            {
                descr = "No timing has been selected";
            }
            else if (ResultCode == ResultCodes.E_NO_INPUT_SELECTED)
            {
                descr = "No input has been selected";
            }
            else if (ResultCode == ResultCodes.E_NO_DISPLAY_INFO_AVAILABLE)
            {
                descr = "No display information available";
            }
            else if (ResultCode == ResultCodes.E_ENGINE_ALREADY_RUNNING)
            {
                descr = "Engine is already running";
            }
            else if (ResultCode == ResultCodes.E_NO_AVAILABLE_ENGINE)
            {
                descr = "There is no available engine";
            }
            else if (ResultCode == ResultCodes.E_NO_DEVICE_SELECTED)
            {
                descr = "No device has been selected";
            }
            else
            {
                descr = "Unknown error";
            }
            return descr;
        }
    }


    public class TreeNode
	{
		public List<string> ValueList { get; private set; } = new List<string>();
		public List<TreeNode> Nodes { get; private set; } = new List<TreeNode>();

		private int offset = 0;
		private StringBuilder buffer = new StringBuilder();

		private void Setup()
		{
			var valueList = buffer.ToString();
			valueList = valueList.TrimStart(' ');
			valueList = valueList.TrimEnd(' ');

			// удаляем двойные пробелы...
			valueList = System.Text.RegularExpressions.Regex.Replace(valueList, @"\s+", " ");

			this.ValueList = new List<string>(valueList.Split(' '));
		}

		public static TreeNode Parse(string data)
		{
			data = data.TrimStart(' ');
			data = data.TrimEnd(' ');

			if (!Validate(data))
			{
				throw new ArgumentException("Invalid arg: " + data);
			}

			return Parse(data.ToArray());

		}

		public static bool Validate(string data)
		{
			if (!data.StartsWith("{") || !data.EndsWith("}"))
			{
				return false;
			}

			// проверяем незакрытые скобки...
			bool result = false;
			Stack<char> stack = new Stack<char>();

			char[] chars = data.ToCharArray();
			for (int i = 0; i < data.Length; i++)
			{
				if (chars[i] == '{')
				{
					stack.Push(chars[i]);
				}
				else if (chars[i] == '}')
				{
					if (stack.Count > 0)
					{
						stack.Pop();
					}
					else
					{
						break;
					}
				}
			}

			if (stack.Count == 0)
			{
				result = true;
			}

			return result;
		}

		private static TreeNode Parse(char[] data, int index = 0)
		{// структуры CP разделяются скобками {}, простые типы пробелами, string в ""
            //создаем дерево в узлах которого будут структуры и данные...
  		    TreeNode node = new TreeNode();
			index++;
			for (int i = index; i < data.Length; i++)
			{
				node.offset++;
				if (data[i] == '}')
				{
					node.Setup();
					return node;
				}

				if (data[i] == '{')
				{
					TreeNode child = Parse(data, i);

					i += child.offset;
					node.offset += child.offset;

					node.Nodes.Add(child);
				}
				else
				{
					node.buffer.Append(data[i]);
				}

			}
			return node;
		}
	}


    public class CPEnvironment
    {
        public const string CPServerAppName = "CPServer.exe";

        public readonly static string ProgramDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        public readonly static string ControlPointPath = System.IO.Path.Combine(ProgramDataPath, "ControlPoint");
        public readonly static string ServerDataFilesPath = System.IO.Path.Combine(ControlPointPath, "ServerDataFiles");

        // Папка в которую сервер сохраняет изображения окон
        //@"%ProgramData%\ControlPoint\ServerDataFiles\images\"
        public readonly static string ImagesPath = System.IO.Path.Combine(ServerDataFilesPath, "images");

        public readonly static Guid Galileo_1_0_Type_Library_CLSID = new Guid("DF2DCD09-6FAE-11D4-94B0-0080C84735F0");
        public readonly static Guid GenieSys_1_0_Type_Library_CLSID = new Guid("D7D93C1A-0C59-4CB0-8C90-BEF34ED55013");

    }

    public class Utils
    {
        public static string LogEnumFlags(Enum flags)
        {
            string log = "";

            Type type = flags.GetType();

            var values = Enum.GetValues(type).Cast<Enum>().Where(f => flags.HasFlag(f));
            log = string.Join("|", values);

            return log;
        }

        public static bool IsRegisteredCLSID(Guid clsid)
        {
            bool success = false;
            var type = Type.GetTypeFromCLSID(clsid, false);

            success = type?.IsCOMObject ?? false;

            return success;
        }

        public static bool CheckAppRegistration(string appName)
        {
            return !string.IsNullOrEmpty(GetRegisterAppFullName(appName));
        }


        public static string GetRegisterAppFullName(string appName)
        {
            var fullName = "";
            try
            {
                var regPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + appName;
                var rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(regPath);
                if (rk != null)
                {
                    var keyVal = rk.GetValue("");
                    if (keyVal != null)
                    {
                        fullName = keyVal.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return fullName;
        }

        public static byte[] BitmapToBytes(Bitmap image, System.Drawing.Imaging.ImageFormat format)
        {
            byte[] bytes = null;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                image.Save(ms, format);
                bytes = ms.ToArray();
            }
            return bytes;
        }
    }

	static class IntParser
	{
		private static Dictionary<Type, Func<string, NumberStyles, object>> dict = new Dictionary<Type, Func<string, NumberStyles, object>>
		{
			{ typeof(byte),   (s, style) => byte.Parse(s, style) },
			{ typeof(sbyte),  (s, style) => sbyte.Parse(s, style) },
			{ typeof(short),  (s, style) => short.Parse(s, style) },
			{ typeof(ushort), (s, style) => ushort.Parse(s, style) },
			{ typeof(int),    (s, style) => int.Parse(s, style) },
			{ typeof(uint),   (s, style) => uint.Parse(s, style) },
			{ typeof(long),   (s, style) => long.Parse(s, style) },
			{ typeof(ulong),  (s, style) => ulong.Parse(s, style) },
		};

		public static T Parse<T>(string str) where T : IComparable, IFormattable, IConvertible
		{
			var parser = dict[typeof(T)];

			var style = NumberStyles.Integer;

			if (str.StartsWith("0x"))
			{
				style = NumberStyles.HexNumber;
				str = str.Replace("0x", "");
			}

			T val = (T)parser(str, style);

			return val;
		}

	}


    public class TraceManager
    {
        private static Dictionary<string, TraceSource> traceSources = new Dictionary<string, TraceSource>();

        private static object syncRoot = new object();

        public static void Init() { }

        public static TraceSource GetTrace(string name)
        {
            TraceSource ts = null;

            lock (syncRoot)
            {
                if (traceSources.ContainsKey(name))
                {
                    ts = traceSources[name];
                }
                else
                {
                    ts = new TraceSource(name);
                    traceSources.Add(name, ts);
                }
            }

            return ts;
        }


        public static void Shutdown()
        {
            lock (syncRoot)
            {
                foreach (var k in traceSources.Keys)
                {
                    var ts = traceSources[k];
                    ts.Close();
                    ts = null;
                }

                traceSources.Clear();
            }

        }
    }

    public static class TraceSourceExtension
    {
        public static void Fatal<T>(this TraceSource ts, T t) where T : Exception
        {
            ts.TraceData(TraceEventType.Critical, 0, t);
        }

        public static void Fatal(this TraceSource ts, string message)
        {
            ts.TraceEvent(TraceEventType.Critical, 0, message);
        }

        public static void Fatal(this TraceSource ts, string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Critical, 0, format, args);
        }

        public static void Error<T>(this TraceSource ts, T t) where T : Exception
        {
            ts.TraceData(TraceEventType.Error, 0, t);
        }

        public static void Error(this TraceSource ts, string message)
        {
            ts.TraceEvent(TraceEventType.Error, 0, message);
        }

        public static void Error(this TraceSource ts, string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Error, 0, format, args);
        }

        public static void Warn<T>(this TraceSource ts, T t) where T : Exception
        {
            ts.TraceData(TraceEventType.Warning, 0, t);
        }

        public static void Warn(this TraceSource ts, string message)
        {
            ts.TraceEvent(TraceEventType.Warning, 0, message);
        }


        public static void Warn(this TraceSource ts, string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Warning, 0, format, args);
        }

        public static void Info<T>(this TraceSource ts, T t) //where T : Exception
        {
            ts.TraceData(TraceEventType.Information, 0, t);
        }

        public static void Info(this TraceSource ts, string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Information, 0, format, args);

        }

        public static void Info(this TraceSource ts, string message)
        {
            ts.TraceEvent(TraceEventType.Information, 0, message);
        }

        public static void Trace<T>(this TraceSource ts, T t) //where T : Exception
        {
            ts.TraceData(TraceEventType.Verbose, 0, t);
        }


        public static void Trace(this TraceSource ts, string message)
        {
            ts.TraceEvent(TraceEventType.Verbose, 0, message);
        }


        public static void Trace(this TraceSource ts, string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Verbose, 0, format, args);
        }



        public static void Debug<T>(this TraceSource ts, T t) //where T : Exception
        {
            ts.TraceData(TraceEventType.Verbose, 0, t);
        }


        public static void Debug(this TraceSource ts, string message)
        {
            ts.TraceEvent(TraceEventType.Verbose, 0, message);
        }


        public static void Debug(this TraceSource ts, string format, params object[] args)
        {
            ts.TraceEvent(TraceEventType.Verbose, 0, format, args);
        }

    }
}
