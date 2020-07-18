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

            logger = LogManager.GetCurrentClassLogger();

            logger.Info("========== START ============");
			//var mediaToolkitPath = @"C:\Users\Alexander\Source\Repos\ScreenStreamer\bin\Debug";
			var mediaToolkitPath = AppDomain.CurrentDomain.BaseDirectory;
			//var mediaToolkitPath = @"Y:\Users\Alexander\source\repos\ScreenStreamer\bin\Debug";

			//var mediaToolkitPath = @"Y:\Users\Alexander\source\repos\ScreenStreamer\bin\Debug";
			//@"C:\Users\Alexander\Source\Repos\ScreenStreamer\bin\Debug";

			if (!System.IO.Directory.Exists(mediaToolkitPath))
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    mediaToolkitPath = dlg.SelectedPath;
                }
            }

            InstanceFactory.EnableLog = true;
            InstanceFactory.Log += (log, level) => 
            {
                if(level == InstanceFactory.LogLevel.Error)
                {
                    logger.Error(log);
                }
                else
                {
                    logger.Debug(log);
                }
                
            };

            if (!MediaToolkitFactory.Startup(mediaToolkitPath))
            {
                MessageBox.Show("Error at MediaToolkit startup:\r\n\r\n" + mediaToolkitPath);

                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var screenCasterControl = MediaToolkitFactory.CreateInstance<IScreenCasterControl>();
            screenCasterControl.ShowDebugPanel = true;
            screenCasterControl.OnSettingsButtonClick += ScreenCasterControl_OnSettingsButtonClick;
            var form = new Form1((Control)screenCasterControl);

            Application.Run(form);

            MediaToolkitFactory.Shutdown();

            logger.Info("========== THE END ============");
        }

        private static void ScreenCasterControl_OnSettingsButtonClick()
        {
            MessageBox.Show("ScreenCasterControl_OnSettingsButtonClick()");
        }
    }
}
