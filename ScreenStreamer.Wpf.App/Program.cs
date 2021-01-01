using MediaToolkit;
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
		private static bool initialized = false;

		static Program()
		{
			if(Utils.ConfigTools.TryGetAppSettingsValue("MediaToolkitPath", out string mediaToolkitPath))
			{
				AssemblyPath = mediaToolkitPath;
				AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
			}
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
                {
                    if (StartupParams.IsElevated)
                    {
                        if (AppManager.RestartAsSystem() > 0)
                        {
                            return 0;
                        }
                        logger.Warn("Restart failed...");
                    }
                    else
                    {
                        AppManager.RunAsSystem();

                        return 0;
                    }
                }


                try
				{
                    logger.Info(StartupParams.GetSysInfo());
					

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
		private static string AssemblyPath = @"..\";
		private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{

			Console.WriteLine("CurrentDomain_AssemblyResolve(...) " + args.Name + " " + args.RequestingAssembly?.ToString() ?? "");
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
					Console.WriteLine(ex.ToString(), LogLevel.Error);
					return null;
				}
			}
			else
			{
				Console.WriteLine("Assembly not found: " + asmFileFullName, LogLevel.Error);
				return null;
			}

			return null;
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
				var hWnd = Kernel32.GetConsoleWindow();
				if (hWnd == IntPtr.Zero)
				{
					Kernel32.AllocConsole();
				}
			}
		}


        public class StartupParameters
        {
            public string UserName { get; private set; } = "";

            public bool IsSystem { get; private set; } = false;
            public bool IsElevated { get; private set; } = false;
            public bool NoRestart { get; private set; } = false;
            public bool RunAsSystem { get; private set; } = false;

			public bool ResetConfig { get; private set; } = false;

			public bool AutoStream { get; private set; } = false;

            public bool IsRemoteSession { get; private set; } = false;
            public bool IsRemotelyControlled { get; private set; } = false;
            public bool IsCompositionEnabled { get; private set; } = false;

            public static StartupParameters Create(string[] args)
            {
                logger.Debug("CommandLine: " + string.Join(" ", args));

                StartupParameters startupParams = new StartupParameters();

                foreach (var arg in args)
                {
                    var _arg = arg?.ToLower();

                    if (_arg == "-norestart")
                    {
                        startupParams.NoRestart = true;
                    }
                    else if (_arg == "-system")
                    {
                        startupParams.RunAsSystem = true;
                    }
					else if (_arg == "-reset")
					{
						startupParams.ResetConfig = true;
					}
					else if (_arg == "-autostream")
                    {
                        startupParams.AutoStream = true;
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
                var sysInfo = Environment.OSVersion + " " + (Environment.Is64BitOperatingSystem ? "x64" : "x86");

               // System.Runtime.InteropServices.RuntimeInformation.OSDescription


                if (StartupParams.IsSystem)
				{// run as system...
					sysInfo += "IsSystem\r\n";
				}
				else if (StartupParams.IsElevated)
				{// запущен с повышеными правами
					sysInfo += "IsElevated\r\n";
				}
				else if (StartupParams.IsRemotelyControlled || StartupParams.IsRemoteSession)
				{// rdp 
					sysInfo += "IsRDP: " + StartupParams.IsRemotelyControlled + ";" + StartupParams.IsRemoteSession + "\r\n";
				}
				else if (!StartupParams.IsCompositionEnabled)
				{// выключена композитная отрисовка
					sysInfo += "DWM Disabled\r\n";
				}


				//...
				return sysInfo;
			}

            public override string ToString()
            {
                return string.Join(";", UserName, IsElevated, IsSystem, NoRestart, IsRemoteSession, IsRemotelyControlled, IsCompositionEnabled);
            }

        }



	}

}
