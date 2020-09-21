using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
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
        public const string CPSecurity = "CPSecurity";

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
        public static string AutoDoubleQuoteString(string value)
        {
            if (!string.IsNullOrEmpty(value) && (value.IndexOf(' ') > -1) && (value.IndexOf('"') == -1))
            {
                if (value.EndsWith(@"\"))
                {
                    value = string.Concat(value, @"\");
                }

                return string.Concat('"', value, '"');
            }

            return value;
        }

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

    public enum ServiceState
    {
        Unknown = -1,
        NotFound = 0,
        Stopped = 1,
        StartPending = 2,
        StopPending = 3,
        Running = 4,
        ContinuePending = 5,
        PausePending = 6,
        Paused = 7
    }

    public static class WinSrvUtil
    {
        public static bool ServiceIsInstalled(string serviceName)
        {
            bool result = false;
            IntPtr scm = OpenSCManager(ScmAccessRights.Connect);

            try
            {
                IntPtr service = OpenService(scm, serviceName, ServiceAccessRights.QueryStatus);
                result = !(service == IntPtr.Zero);

                CloseServiceHandle(service);
            }
            finally
            {
                CloseServiceHandle(scm);
            }

            return result;
        }

        public static ServiceState GetServiceStatus(string serviceName)
        {
            IntPtr hSCManager = OpenSCManager(ScmAccessRights.Connect);

            try
            {
                IntPtr hService = OpenService(hSCManager, serviceName, ServiceAccessRights.QueryStatus);
                if (hService == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    Marshal.ThrowExceptionForHR(errorCode);
                }

                try
                {
                    var status = GetServiceStatus(hService);

                    return status.dwCurrentState;
                }
                finally
                {
                    CloseServiceHandle(hService);
                }
            }
            finally
            {
                CloseServiceHandle(hSCManager);
            }
        }

        public static void StartService(string serviceName)
        {

            IntPtr hSCManager = OpenSCManager(ScmAccessRights.Connect);
            try
            {
                IntPtr hService = OpenService(hSCManager, serviceName, ServiceAccessRights.QueryStatus | ServiceAccessRights.Start);
                if (hService == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    Marshal.ThrowExceptionForHR(errorCode);
                }

                try
                {
                    StartService(hService);
                }
                finally
                {
                    CloseServiceHandle(hService);
                }
            }
            finally
            {
                CloseServiceHandle(hSCManager);
            }

        }

        public static void StopService(string serviceName)
        {

            IntPtr hSCManager = OpenSCManager(ScmAccessRights.Connect);
            try
            {
                IntPtr hService = OpenService(hSCManager, serviceName, ServiceAccessRights.QueryStatus | ServiceAccessRights.Stop);

                if (hService == IntPtr.Zero)
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    Marshal.ThrowExceptionForHR(errorCode);
                }

                try
                {
                    StopService(hService);
                }
                finally
                {
                    CloseServiceHandle(hService);
                }
            }
            finally
            {
                CloseServiceHandle(hSCManager);
            }
        }

        private static void StartService(IntPtr hService)
        {
            SERVICE_STATUS status = new SERVICE_STATUS();
            StartService(hService, 0, 0);

            var changedStatus = WaitForServiceStatus(hService, ServiceState.StartPending, ServiceState.Running);
            if (!changedStatus)
            {
                throw new InvalidOperationException("Unable to start service");
            }

        }

        private static void StopService(IntPtr service)
        {
            SERVICE_STATUS status = new SERVICE_STATUS();
            ControlService(service, ServiceControl.Stop, status);
            var changedStatus = WaitForServiceStatus(service, ServiceState.StopPending, ServiceState.Stopped);
            if (!changedStatus)
            {
                throw new InvalidOperationException("Unable to stop service");
            }

        }


        private static SERVICE_STATUS GetServiceStatus(IntPtr hService)
        {
            SERVICE_STATUS status = new SERVICE_STATUS();

            if (QueryServiceStatus(hService, status) == 0)
            {
                var errorCode = Marshal.GetLastWin32Error();
                Marshal.ThrowExceptionForHR(errorCode);
            }

            return status;
        }

        private static bool WaitForServiceStatus(IntPtr hService, ServiceState waitStatus, ServiceState desiredStatus)
        {

            SERVICE_STATUS status = GetServiceStatus(hService);

            if (status.dwCurrentState == desiredStatus)
            {
                return true;
            }


            int dwStartTickCount = Environment.TickCount;
            int dwOldCheckPoint = status.dwCheckPoint;

            while (status.dwCurrentState == waitStatus)
            {
                int dwWaitTime = status.dwWaitHint / 10;

                if (dwWaitTime < 1000) dwWaitTime = 1000;
                else if (dwWaitTime > 10000) dwWaitTime = 10000;

                Thread.Sleep(dwWaitTime);

                status = GetServiceStatus(hService);

                if (status.dwCheckPoint > dwOldCheckPoint)
                {
                    dwStartTickCount = Environment.TickCount;
                    dwOldCheckPoint = status.dwCheckPoint;
                }
                else
                {
                    if (Environment.TickCount - dwStartTickCount > status.dwWaitHint)
                    {
                        throw new System.TimeoutException();
                    }
                }
            }

            return (status.dwCurrentState == desiredStatus);
        }

        private static IntPtr OpenSCManager(ScmAccessRights rights)
        {
            IntPtr scHandle = OpenSCManager(null, null, rights);
            if (scHandle == IntPtr.Zero)
            {
                var errorCode = Marshal.GetLastWin32Error();
                Marshal.ThrowExceptionForHR(errorCode);
            }

            return scHandle;
        }

        #region NativeMethods

        [Flags]
        public enum ScmAccessRights
        {
            Connect = 0x0001,
            CreateService = 0x0002,
            EnumerateService = 0x0004,
            Lock = 0x0008,
            QueryLockStatus = 0x0010,
            ModifyBootConfig = 0x0020,
            StandardRightsRequired = 0xF0000,
            AllAccess = (StandardRightsRequired | 
                Connect | 
                CreateService | 
                EnumerateService |
                Lock |
                QueryLockStatus | 
                ModifyBootConfig)
        }

        [Flags]
        public enum ServiceAccessRights
        {
            QueryConfig = 0x1,
            ChangeConfig = 0x2,
            QueryStatus = 0x4,
            EnumerateDependants = 0x8,
            Start = 0x10,
            Stop = 0x20,
            PauseContinue = 0x40,
            Interrogate = 0x80,
            UserDefinedControl = 0x100,
            Delete = 0x00010000,
            StandardRightsRequired = 0xF0000,
            AllAccess = (StandardRightsRequired | 
                QueryConfig | 
                ChangeConfig |
                QueryStatus |
                EnumerateDependants |
                Start | 
                Stop | 
                PauseContinue | 
                Interrogate | 
                UserDefinedControl)
        }

        public enum ServiceBootFlag
        {
            Start = 0x00000000,
            SystemStart = 0x00000001,
            AutoStart = 0x00000002,
            DemandStart = 0x00000003,
            Disabled = 0x00000004
        }

        public enum ServiceControl
        {
            Stop = 0x00000001,
            Pause = 0x00000002,
            Continue = 0x00000003,
            Interrogate = 0x00000004,
            Shutdown = 0x00000005,
            ParamChange = 0x00000006,
            NetBindAdd = 0x00000007,
            NetBindRemove = 0x00000008,
            NetBindEnable = 0x00000009,
            NetBindDisable = 0x0000000A
        }

        public enum ServiceError
        {
            Ignore = 0x00000000,
            Normal = 0x00000001,
            Severe = 0x00000002,
            Critical = 0x00000003
        }

        private const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        private const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;

        [StructLayout(LayoutKind.Sequential)]
        private class SERVICE_STATUS
        {
            public int dwServiceType = 0;
            public ServiceState dwCurrentState = 0;
            public int dwControlsAccepted = 0;
            public int dwWin32ExitCode = 0;
            public int dwServiceSpecificExitCode = 0;
            public int dwCheckPoint = 0;
            public int dwWaitHint = 0;
        }

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr OpenSCManager(string machineName, string databaseName, ScmAccessRights dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, ServiceAccessRights dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseServiceHandle(IntPtr hSCObject);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int QueryServiceStatus(IntPtr hService, SERVICE_STATUS lpServiceStatus);

        [DllImport("advapi32.dll")]
        private static extern int ControlService(IntPtr hService, ServiceControl dwControl, SERVICE_STATUS lpServiceStatus);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int StartService(IntPtr hService, int dwNumServiceArgs, int lpServiceArgVectors); 
        #endregion

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
