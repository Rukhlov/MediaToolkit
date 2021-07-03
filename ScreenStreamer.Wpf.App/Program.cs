﻿using MediaToolkit;
using MediaToolkit.NativeAPIs;

using NLog;
using ScreenStreamer.Common;
using ScreenStreamer.Wpf.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenStreamer.Wpf
{
	public class Program
	{
		private static Logger logger = null;
		public static StartupParameters StartupParams { get; private set; }
        public static bool TestMode => StartupParams?.TestMode ?? false;

#if DEBUG
        public static readonly bool DebugMode = true;
#else
        public static readonly bool DebugMode = false;
#endif

		private static bool initialized = false;

		static Program()
		{
			if(Utils.ConfigTools.TryGetAppSettingsValue("MediaToolkitPath", out string mediaToolkitPath))
			{// если переопределен путь к .\MediaToolkit 
				AssemblyPath = mediaToolkitPath;
				AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolveFromResources;


        }



        [STAThread]
        public static int Main(string[] args)
        {
            int exitCode = 0;

            InitLogger();

            logger.Info("============================ START ============================");

			bool createdNew = false;
            Mutex mutex = null;
            try
            {
                StartupParams = StartupParameters.Create(args);

                mutex = new Mutex(true, AppConsts.ApplicationId, out createdNew);
                if (!createdNew)
                {
                    logger.Info("Another instance is already running...");
					if (!Models.AppModel.AllowMutipleInstance)
					{
						var res = WndProcService.ShowAnotherInstance();
						return 0;

						//return -1;
						//...
					}
				}


                if (StartupParams.RunAsSystem)
				{// что бы можно было захватывать защищенный рабочий стол с LogonScreen-ом и UAC 
					//запускаем правами SYSTEM

                    if (StartupParams.IsElevated)
                    {// если права повышеные то можно запустить процесс с правами системы
						if (AppManager.RestartAsSystem(args) > 0)
                        {
                            return 0;
                        }
                        logger.Warn("Restart as system failed...");
                    }
                    else
                    {// сначала получаем права админа...
                        AppManager.RunAsSystem(args);

                        return 0;
                    }
                }


                try
                {
                    LogSystemInfo();

                    MediaToolkitManager.Startup();
                    Shcore.SetProcessPerMonitorDpiAwareness();

                    var application = new App();
                    application.DispatcherUnhandledException += Application_DispatcherUnhandledException;
                    application.InitializeComponent();
                    initialized = true;

                    logger.Info("============================ RUN ============================");
                    application.Run();
                }
                finally
				{
					MediaToolkitManager.Shutdown();
				}
            }
            catch(Exception ex)
            {
				ProcessError(ex);
			}
            finally
            {
                if (mutex != null)
                {
                    if (createdNew)
                    {
                        mutex.ReleaseMutex();
                    }
                    mutex.Dispose();
                }

                logger.Info("============================ THE END ============================");
            }

           
            return exitCode;
        }

        private static void LogSystemInfo()
        {
            logger.Trace("LogSystemInfo()");

            //var sysInfo = "OS: "  + Environment.OSVersion + " " + (Environment.Is64BitOperatingSystem ? "x64" : "x86");
            logger.Info("OS: " + Utils.SystemInfo.LogOSInfo());
            logger.Info("CPU: " + Utils.SystemInfo.LogProcessorInfo());
            logger.Info("RAM: " + Utils.SystemInfo.LogMemoryInfo());
            logger.Info("GPU:\r\n" + Utils.SystemInfo.LogGpuInfo());

            if (StartupParams.IsSystem)
            {
                logger.Info("Running as SYSTEM: " + StartupParams.IsSystem);
            }

            if (StartupParams.IsElevated)
            {
                logger.Info("Running as Admin: " + StartupParams.IsElevated);
            }

            // rdp 
            if (StartupParams.IsRemotelyControlled)
            {
                logger.Info("Remotely Controlled: " + StartupParams.IsRemotelyControlled);
            }

            if (StartupParams.IsRemoteSession)
            {
                logger.Info("Remote Session: " + StartupParams.IsRemoteSession);
            }
            
            if (!StartupParams.IsCompositionEnabled)
            {// выключена композитная отрисовка может быть только для Win7
                logger.Info("DWM: " + StartupParams.IsCompositionEnabled);
            }


            
        }

        private static string AssemblyPath = @"..\";
		private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			OnLog("CurrentDomain_AssemblyResolve(...) " + args.Name + " " + args.RequestingAssembly?.ToString() ?? "", LogLevel.Trace);

			var asmName = args.Name;

			if (asmName.Contains(".resources"))
			{
				return null;
			}

			Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == asmName);
			if (assembly != null)
			{
				return assembly;
			}

			string filename = asmName.Split(',')[0];
			string asmFileFullName = Path.Combine(AssemblyPath, (filename + ".dll"));
			if (!File.Exists(asmFileFullName))
			{
				asmFileFullName = Path.Combine(AssemblyPath, (filename + ".exe"));
			}

			if (File.Exists(asmFileFullName))
			{
				try
				{
					return Assembly.LoadFrom(asmFileFullName);
				}
				catch (Exception ex)
				{
					OnLog(ex.ToString(), LogLevel.Error);
					return null;
				}
			}
			else
			{
				OnLog("Assembly not found: " + asmFileFullName, LogLevel.Error);

				return null;
			}

			return null;
		}

        private static Assembly CurrentDomain_AssemblyResolveFromResources(object sender, ResolveEventArgs args)
        {
            string traceLog = "Trying to resolve: " + args.Name;

            Assembly targetAssembly = null;
            try
            {
                var thisAssembly = Assembly.GetExecutingAssembly();
                string embeddedResources = new AssemblyName(thisAssembly.FullName).Name + ".Embedded";
                //const string embeddedResources = "ScreenStreamer.Wpf.App.Embedded";

                var targetName = new AssemblyName(args.Name).Name;
                var resName = embeddedResources + "." + targetName + ".dll";

                using (var stream = thisAssembly.GetManifestResourceStream(resName))
                {
                    if (stream != null)
                    {
                        byte[] assemblyData = new byte[stream.Length];

                        stream.Read(assemblyData, 0, assemblyData.Length);
                        targetAssembly = Assembly.Load(assemblyData);
                    }
                }
                traceLog += targetAssembly != null ? " OK" : " Miss";
            }
            catch(Exception ex)
            {
                OnLog(ex.Message, LogLevel.Error);
                traceLog += " Failed";
            }

            OnLog(traceLog, LogLevel.Trace);

            return targetAssembly;
        }

        public static void OnLog(string traceLog, LogLevel logLevel)
        {
			if (logger != null)
			{
				logger.Log(logLevel, traceLog);
			}
			else
			{
				Console.WriteLine(traceLog);
			}

		}

        private static void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			logger.Debug("Application_DispatcherUnhandledException(...)");

			e.Handled = true;

			ProcessError(e.Exception);

		}

		private static void ProcessError(Exception ex)
		{
			//TODO: process error...
			//...

			var message = "An unexpected error has occurred. Application will be closed";

			try
			{
				var exceptionMessage = ex?.Message??message;

				logger.Fatal(exceptionMessage);

				if (initialized)
				{
					var dialogService = new DialogService();

					var vm = new ViewModels.Dialogs.MessageBoxViewModel(exceptionMessage, "Error", MessageBoxButton.OK);
					var result = dialogService.ShowDialog(vm);
				}
				else
				{
					MessageBox.Show(exceptionMessage, AppConsts.ApplicationCaption, MessageBoxButton.OK, MessageBoxImage.Error);
				}

			}
			catch (Exception ex1)
			{
				logger.Error(ex1);
				MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}


		private static void InitLogger()
		{

            var config = LogManager.Configuration;
            if (config != null)
            {
                var vars = config.Variables;
                if (vars != null)
                {
                    if (!vars.ContainsKey("AppConfigPath"))
                    {
                        vars.Add("AppConfigPath", ConfigManager.ConfigPath);
                    }

                }
            }

            logger = LogManager.GetCurrentClassLogger();

			var logFactory = logger.Factory;
			if (logFactory == null)
			{
				return;
			}

			var logConfig = logFactory.Configuration;
			if (logConfig == null)
			{
				return;
			}

			var logRules = logConfig.LoggingRules;
			if (logRules == null)
			{
				return;
			}

			bool needConsole = false;
			foreach (var rule in logRules)
			{
				var targets = rule.Targets;
				if (targets == null)
				{
					continue;
				}

				foreach (var target in targets)
				{
					var targetName = target.Name;
					if (targetName == "console")
					{
						needConsole = true;
						break;
					}
				}

				if (needConsole)
				{
					break;
				}
			}

			if (needConsole)
			{
                Utils.WinConsole.AllocConsole();
            }
        }


        public class StartupParameters
        {
			class CommandLineArgs
			{
				public const string NoRestart = "-norestart";
				public const string System = "-system";
				public const string Reset = "-reset";
				public const string Test = "-test";
				public const string AutoStream = "-autostream";
				public const string Lang = "-lang=";
				public const string Console = "-console";
			}
			

			public string UserName { get; private set; } = "";

            public bool IsSystem { get; private set; } = false;
            public bool IsElevated { get; private set; } = false;
            public bool NoRestart { get; private set; } = false;
            public bool RunAsSystem { get; private set; } = false;

			public bool ResetConfig { get; private set; } = false;

			public bool AutoStream { get; private set; } = false;
            public bool TestMode { get; private set; } = false;
            //public bool AllocConsole { get; private set; } = false;

            public bool IsRemoteSession { get; private set; } = false;
            public bool IsRemotelyControlled { get; private set; } = false;
            public bool IsCompositionEnabled { get; private set; } = false;

			public string ActiveCulture { get; private set; } = "";

			public static StartupParameters Create(string[] args)
            {
                logger.Debug("CommandLine: " + string.Join(" ", args));

                StartupParameters startupParams = new StartupParameters();

                foreach (var arg in args)
                {
                    var _arg = arg?.ToLower();

                    if (_arg == CommandLineArgs.NoRestart)
                    {
                        startupParams.NoRestart = true;
                    }
                    else if (_arg == CommandLineArgs.System)
                    {
                        startupParams.RunAsSystem = true;
                    }
					else if (_arg == CommandLineArgs.Reset)
					{
						startupParams.ResetConfig = true;
					}
					//else if (_arg == CommandLineArgs.Console)
					//{
					//    startupParams.AllocConsole = true;
					//}
					else if (_arg == CommandLineArgs.Test)
                    {
                        startupParams.TestMode = true;
                    }
					else if (_arg == CommandLineArgs.AutoStream)
                    {
                        startupParams.AutoStream = true;
                    }
					else if (_arg.StartsWith(CommandLineArgs.Lang))
					{
						var len = CommandLineArgs.Lang.Length;
						if (_arg.Length > len)
						{
							startupParams.ActiveCulture= _arg.Substring(len);

						}						
					}
					else
                    {
                        //...
                    }
                }

                using (var wi = System.Security.Principal.WindowsIdentity.GetCurrent())
                {
                    startupParams.UserName = wi.Name;
                    startupParams.IsElevated = (wi.Owner != wi.User);
                    startupParams.IsSystem = wi.IsSystem;
                }

                startupParams.IsRemoteSession = SystemParameters.IsRemoteSession;
                startupParams.IsRemotelyControlled = SystemParameters.IsRemotelyControlled;
                startupParams.IsCompositionEnabled = DwmApi.IsCompositionEnabled();

                return startupParams;
            }

			public string GetSysInfo()
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("");
                var sysInfo = "OS: " + Utils.SystemInfo.LogOSInfo();// + Environment.OSVersion + " " + (Environment.Is64BitOperatingSystem ? "x64" : "x86");


                sb.AppendLine(sysInfo);
				// System.Runtime.InteropServices.RuntimeInformation.OSDescription

				var processInfo = "CPU: " + Utils.SystemInfo.LogProcessorInfo();
				sb.AppendLine(processInfo);

				var memoryInfo = "RAM: " + Utils.SystemInfo.LogMemoryInfo();
				sb.AppendLine(memoryInfo);


                var gpuInfo = "GPU: " + Utils.SystemInfo.LogGpuInfo();
                sb.AppendLine(gpuInfo);

                // run as system...
                sb.AppendLine("Running as SYSTEM: " + StartupParams.IsSystem);
				// запущен с повышеными правами
				sb.AppendLine("Running as Admin: " + StartupParams.IsElevated);

				if (StartupParams.IsRemotelyControlled || StartupParams.IsRemoteSession)
				{// rdp 
					sb.AppendLine("RDP: " + StartupParams.IsRemotelyControlled + ";" + StartupParams.IsRemoteSession);
				}

				// композитная отрисовка
				sb.AppendLine("DWM: " + StartupParams.IsCompositionEnabled);

				return sb.ToString();
			}

            public override string ToString()
            {
                return string.Join(";", UserName, IsElevated, IsSystem, NoRestart, IsRemoteSession, IsRemotelyControlled, IsCompositionEnabled);
            }

        }



	}

}
