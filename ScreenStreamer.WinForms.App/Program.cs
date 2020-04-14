using MediaToolkit;
using MediaToolkit.NativeAPIs;
using NLog;
using ScreenStreamer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenStreamer.WinForms.App
{
    class Program
    {
        private static Logger logger = null;

        public static StartupParameters StartupParams { get; private set; }

        [STAThread]
        static void Main(string[] args)
        {

            logger = LogManager.GetCurrentClassLogger();

            logger.Info("========== START ============");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            StartupParams = StartupParameters.Create(args);


            if (StartupParams.IsElevated)
            {
                if (!StartupParams.NoRestart)
                {//Restart application with system permissions...
                    if (RestartElevated() > 0)
                    {
                        return;
                    }
                    logger.Warn("Restart failed...");
                    // открываем процесс как обычно...
                    //TODO:...
                }
            }



            bool createdNew = false;
            Mutex mutex = null;
            try
            {
                mutex = new Mutex(true, AppConsts.ApplicationId, out createdNew);
                if (!createdNew)
                {
                    logger.Info("Another instance is already running...");
                    //...
                }



                bool tempMode = !createdNew;
                Config.Initialize(tempMode);

                MediaToolkitManager.Startup();

                //DwmApi.DisableAero(true);
                Shcore.SetProcessPerMonitorDpiAwareness();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                MainForm form = new MainForm();

                logger.Info("========== RUN ============");
                Application.Run(form);

            }
            finally
            {
                Config.Shutdown();

                MediaToolkitManager.Shutdown();

                if (mutex != null)
                {
                    if (createdNew)
                    {
                        mutex.ReleaseMutex();
                    }
                    mutex.Dispose();
                }

                logger.Info("========== THE END ============");
            }

        }

        private static int RestartElevated()
		{// что бы можно было переключится на защищенные рабочие столы (Winlogon, ScreenSaver)
		 // перезапускам процесс с системными правами
			logger.Debug("RestartElevated()");

            int pid = 0;
            try
            {
                var applicationDir = System.IO.Directory.GetCurrentDirectory();
                var applicationName = AppDomain.CurrentDomain.FriendlyName;

                var applicatonFullName = System.IO.Path.Combine(applicationDir, applicationName);

                var commandLine = "-norestart";

     
                pid = MediaToolkit.NativeAPIs.Utils.ProcessTool.StartProcessWithSystemToken(applicatonFullName, commandLine);

                if (pid > 0)
                {
                    using (var process = System.Diagnostics.Process.GetProcessById(pid))
                    {
                        if (process != null)
                        {
                            logger.Info("New process started: " + process.ProcessName + " " + process.Id);
                        }
                    }
                }
                else
                {
                    //...
                    //throw new Exception()
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
               // throw;
            }

            return pid;
        }

        public class StartupParameters
        {
            public string UserName { get; private set; } = "";

            public bool IsSystem { get; private set; } = false;
            public bool IsElevated { get; private set; } = false;
            public bool NoRestart { get; private set; } = false;

            public static StartupParameters Create(string[] args)
            {
                logger.Debug("CommandLine: " + string.Join(" ", args));

                StartupParameters startupParams = new StartupParameters();

                foreach (var arg in args)
                {
                    if (arg?.ToLower() == "-norestart")
                    {
                        startupParams.NoRestart = true;
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

                    logger.Info(startupParams.ToString());
                }

                return startupParams;
            }

            public override string ToString()
            {
                return string.Join(";", UserName, IsElevated, IsSystem, NoRestart);
            }
        }


        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Debug("CurrentDomain_UnhandledException(...)");

            Exception ex = null;

            var obj = e.ExceptionObject;
            if (obj != null)
            {
                logger.Fatal(obj);
                ex = obj as Exception;
            }

            string errorMessage = "Unexpected Error";
            if (ex != null)
            {
                errorMessage = ex.Message;
            }

             MessageBox.Show(errorMessage);
        }
    }


}
