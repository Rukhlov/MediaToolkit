using MediaToolkit.SharedTypes;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.PolywallClient
{

    static class Program
    {
     
        private static Logger logger = null;

        //private static IMediaToolkit mediaToolkit = null;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {

            try
            {
                logger = LogManager.GetCurrentClassLogger();

                logger.Info("========== START ============");
                //var mediaToolkitPath = @"C:\Users\Alexander\Source\Repos\ScreenStreamer\bin\Debug";
                var mediaToolkitPath = AppDomain.CurrentDomain.BaseDirectory;
                //var mediaToolkitPath = @"Y:\Users\Alexander\source\repos\ScreenStreamer\bin\Debug";

                //var mediaToolkitPath = @"Y:\Users\Alexander\source\repos\ScreenStreamer\bin\Debug";
                //@"C:\Users\Alexander\Source\Repos\ScreenStreamer\bin\Debug";

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);


                if (!System.IO.Directory.Exists(mediaToolkitPath))
                {
                    FolderBrowserDialog dlg = new FolderBrowserDialog();
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        mediaToolkitPath = dlg.SelectedPath;
                    }
                }


                if (!MediaManager.Startup(mediaToolkitPath))
                {
                    MessageBox.Show("Error at MediaToolkit startup:\r\n\r\n" + mediaToolkitPath);

                    return;
                }


                var screenCasterControl = MediaManager.CreateInstance<IScreenCasterControl>();
                screenCasterControl.ShowDebugPanel = true;
                screenCasterControl.OnSettingsButtonClick += () =>
                {
                    MessageBox.Show("OnSettingsButtonClick()");

                    screenCasterControl.AspectRatio = !screenCasterControl.AspectRatio;
                };

                var form = new Form1((Control)screenCasterControl);

                Application.Run(form);

            }
            finally
            {

                MediaManager.Shutdown();

                logger.Info("========== THE END ============");
            }

        }


        class MediaManager
        {
            private static IMediaToolkitBootstrapper mediaToolkit = null;
            public static bool IsStarted { get; private set; }

            public static bool Startup(string assemblyPath = "", bool throwExceptions = false)
            {
                if (IsStarted)
                {
                    if (!throwExceptions)
                    {
                        return false;
                    }

                    throw new InvalidOperationException("IsStarted  " + IsStarted);
                }


                InstanceFactory.EnableLog = true;
                InstanceFactory.Log += (log, level) =>
                {
                    if (level == InstanceFactory.LogLevel.Error)
                    {
                        logger?.Error(log);
                    }
                    else
                    {
                        logger?.Debug(log);
                    }

                };

                try
                {
                    InstanceFactory.AssemblyPath = assemblyPath;

                    InstanceFactory.RegisterType<IMediaToolkitBootstrapper>("MediaToolkit.dll");

                    InstanceFactory.RegisterType<IVideoRenderer>("MediaToolkit.dll");
                    InstanceFactory.RegisterType<IAudioRenderer>("MediaToolkit.dll");
                    InstanceFactory.RegisterType<IMediaRenderSession>("MediaToolkit.dll");


                    if(!TryGetAppSettingsValue("ScreenCasterClassName", out string className))
                    {
                        className = "ScreenCastControl";
                    }

                    InstanceFactory.RegisterType<IScreenCasterControl>("MediaToolkit.UI.dll", className);
                    //InstanceFactory.RegisterType<IScreenCasterControl>("MediaToolkit.UI.dll", "ScreenCastControl");

                    mediaToolkit = InstanceFactory.CreateInstance<IMediaToolkitBootstrapper>();

                    mediaToolkit.Startup();

                    IsStarted = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    //Debug.Fail(ex.Message);
                    if (throwExceptions)
                    {
                        throw;
                    }
                }

                return IsStarted;
            }

            public static T CreateInstance<T>(object[] args = null, bool throwExceptions = false) where T : class
            {
                if (!IsStarted)
                {
                    if (!throwExceptions)
                    {
                        return null;
                    }

                    throw new InvalidOperationException("IsStarted  " + IsStarted);
                }

                return InstanceFactory.CreateInstance<T>(args, throwExceptions);
            }

            public static void Shutdown()
            {
                if (mediaToolkit != null)
                {
                    mediaToolkit.Shutdown();
                }
            }


        }



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
                logger.Debug(ex.Message);
            }


            return Result;
        }

        public static bool TryGetValueFromCollection<T>(System.Collections.Specialized.NameValueCollection settings, string paramName, out T t)
        {
            logger.Trace("TryGetValueFromCollection(...) " + paramName);
            bool Result = false;

            t = default(T);
            if (settings == null)
            {
                logger.Trace("TryGetParams(...) settings == null");

                return Result;
            }

            if (string.IsNullOrEmpty(paramName))
            {
                logger.Trace("TryGetParams(...) paramName == null");

                return Result;
            }

            if (settings.Count <= 0)
            {

                logger.Trace("TryGetParams(...) settings.Count <= 0");

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
                    logger.Trace(paramName + " = " + val);

                    var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
                    if (converter != null)
                    {
                        t = (T)converter.ConvertFromString(val);
                        Result = true;
                    }
                }
                else
                {
                    logger.Trace(paramName + " not found");
                }

            }
            catch (Exception ex)
            {
                logger.Debug(ex.Message);
            }

            return Result;
        }

    }
}
