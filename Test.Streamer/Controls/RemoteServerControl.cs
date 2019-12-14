using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;

namespace TestStreamer.Controls
{

    public partial class RemoteServerControl : UserControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public RemoteServerControl()
        {
            InitializeComponent();
        }


        private RemoteDesktopService desktopEngine = null;

        public bool ServiceHostOpened
        {
            get
            {
                return (desktopEngine != null && desktopEngine.IsOpened);
            }
        }


        private void startRemoteServButton_Click(object sender, EventArgs e)
        {
            logger.Debug("startRemoteServButton_Click(...)");
            var _addr = textBox1.Text;
            this.Enabled = false;
            //panel1.Enabled = false;
            //this.UseWaitCursor = true;
            this.Cursor = Cursors.WaitCursor;

            var task = Task.Run(() =>
            {
                desktopEngine = new RemoteDesktopService();

                var address = "net.tcp://" + _addr + "/RemoteDesktop";

                desktopEngine.Open(address);

            });
            task.ContinueWith((t) =>
            {
                UpdateControls();


                this.Enabled = true;
                //panel1.Enabled = true;

                this.Cursor = Cursors.Default;

                //this.UseWaitCursor = false;
                //stopRemoteServButton.Select();

                //this.ActiveControl = this.stopRemoteServButton;
                this.stopRemoteServButton.Focus();

                //Application.DoEvents();


            }, TaskScheduler.FromCurrentSynchronizationContext());


        }

        private void stopRemoteServButton_Click(object sender, EventArgs e)
        {
            logger.Debug("stopRemoteServButton_Click(...)");
            this.Enabled = false;
            // panel1.Enabled = false;
            this.Cursor = Cursors.WaitCursor;

            var task = Task.Run(() =>
            {
                if (desktopEngine != null)
                {
                    desktopEngine.Close();
                    desktopEngine = null;
                }


            });

            task.ContinueWith((t) =>
            {


                UpdateControls();

                this.Enabled = true;
                // panel1.Enabled = true;
                this.Cursor = Cursors.Default;


            }, TaskScheduler.FromCurrentSynchronizationContext());

        }


        private void UpdateControls()
        {

            //this.fpsNumeric2.Enabled = !ServiceHostOpened;
            //this.inputSimulatorCheckBox2.Enabled = !ServiceHostOpened;
            //this.screensComboBox2.Enabled = !ServiceHostOpened;
            //this.screensUpdateButton2.Enabled = !ServiceHostOpened;

            this.startRemoteServButton.Enabled = !ServiceHostOpened;
            this.stopRemoteServButton.Enabled = ServiceHostOpened;


        }
    }
}
