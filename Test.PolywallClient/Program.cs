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

            var mediaToolkitPath = @"C:\Users\Alexander\Source\Repos\ScreenStreamer\bin\Debug";

            if (!System.IO.Directory.Exists(mediaToolkitPath))
            {
                FolderBrowserDialog dlg = new FolderBrowserDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    mediaToolkitPath = dlg.SelectedPath;
                }
            }

            if (!MediaToolkitFactory.Startup(mediaToolkitPath))
            {
                MessageBox.Show("Error at MediaToolkit startup:\r\n\r\n" + mediaToolkitPath);

                return;
            }

            //var fileFullName = pluginPath + @"\MediaToolkit.UI.dll";

            //InstanceFactory.AssemblyPath = pluginPath;

            //InstanceFactory.RegisterType<IMediaToolkit>("MediaToolkit.dll");
            //InstanceFactory.RegisterType<IScreenCasterControl>("MediaToolkit.UI.dll");

            //mediaToolkit = InstanceFactory.CreateInstance<IMediaToolkit>();

            //mediaToolkit.Startup();
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //var control = InstanceFactory.CreateInstance<IScreenCasterControl>();

            var control = MediaToolkitFactory.CreateInstance<IScreenCasterControl>();

            var form = new Form1((Control)control);

            Application.Run(form);

            MediaToolkitFactory.Shutdown();

            logger.Info("========== THE END ============");
        }
    }
}
