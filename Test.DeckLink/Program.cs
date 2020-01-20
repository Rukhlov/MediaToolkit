using MediaToolkit.SharedTypes;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.DeckLink
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

            var mediaToolkitPath = AppDomain.CurrentDomain.BaseDirectory;

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
                if (level == InstanceFactory.LogLevel.Error)
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

            var control = MediaToolkitFactory.CreateInstance<IDeckLinkInputControl>();

            control.CaptureStarted += () =>
            {

            };

            control.CaptureStopped += () =>
            {
                var code = control.Code;
                if (code != 0)
                {
                    var result = MessageBox.Show("Device stopped with error: " + code, "",
                        MessageBoxButtons.RetryCancel);

                    if (result == DialogResult.Retry)
                    {
                        var deviceIndex = control.DeviceIndex;

                        control.StartCapture(deviceIndex);

                        return;
                    }

                }
            };


            control.DebugMode = true;

            var form = new Form
            {
                Size = new System.Drawing.Size(1280, 720),
                StartPosition = FormStartPosition.CenterScreen,
            };

            form.FormClosed += (o, a) =>
            {

                if (control != null)
                {
                    control.StopCapture();
                }
            };

            var c = (Control)control;
            c.Dock = DockStyle.Fill;
            form.Controls.Add(c);


            Application.Run(form);

            MediaToolkitFactory.Shutdown();

            logger.Info("========== THE END ============");
        }

    }

    ///// <summary>
    ///// Главная точка входа для приложения.
    ///// </summary>
    //[STAThread]
    //// [MTAThread]
    //static void Main()
    //{

    //    MediaToolkit.MediaToolkitManager.Startup();

    //    Application.EnableVisualStyles();
    //    Application.SetCompatibleTextRenderingDefault(false);
    //    Application.Run(new Form2());


    //    MediaToolkit.MediaToolkitManager.Shutdown();
    //}
    //}
}
