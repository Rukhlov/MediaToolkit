using MediaToolkit;
using MediaToolkit.Common;
using MediaToolkit.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestStreamer.Controls;

namespace TestStreamer
{
    public partial class MainForm : Form
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MainForm()
        {
            InitializeComponent();


            this.notifyIcon.Visible = true;

            this.notifyIcon.Text = "TEST_TEXT_TEST_TEXT_TEST_TEXT\r\nTEST_TEXT_TEST_TEXT_TEST_TEXT\r\n";


            UpdateScreens();

            //var hostName = System.Net.Dns.GetHostName();
            //textBox1.Text = @"net.tcp://" + hostName + @"/RemoteDesktop";

            //textBox1.Text = @"net.tcp://0.0.0.0/RemoteDesktop";

            UpdateNetworks();

            UpdateControls();


        }

        private bool allowshowdisplay = true;

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(allowshowdisplay ? value : allowshowdisplay);
        }

        private BindingList<ComboBoxItem> screenItems = new BindingList<ComboBoxItem>();

        class ScreenItem
        {
            public string Title { get; set; }
            public Screen Screen { get; set; }

        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.allowshowdisplay = true;

                this.Visible = !this.Visible;
            }

        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = true;
        }

        private void exitButton_Click(object sender, EventArgs e)
        {

            closing = true;
            this.Close();

            //Application.Exit();

        }


        protected override void OnClosing(CancelEventArgs e)
        {
            if (!closing)
            { 
                e.Cancel = true;
                this.Visible = false;
            }
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
           
            base.OnClosed(e);
        }

        private bool closing = false;
        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            closing = true;
            this.Close();
        }

        private void screensUpdateButton_Click(object sender, EventArgs e)
        {
            UpdateScreens();
        }

        private void UpdateScreens()
        {
            var screens = Screen.AllScreens.Select(s => new ComboBoxItem { Name = s.DeviceName, Tag = s }).ToList();

            screenItems = new BindingList<ComboBoxItem>(screens);

            screensComboBox.DisplayMember = "Name";
            screensComboBox.DataSource = screenItems;
        }

        private bool isStreaming = false;

        private ScreenStreamer videoStreamer = null;
        private ScreenSource screenSource = null;
        private DesktopManager desktopMan = null;

        private void startButton_Click(object sender, EventArgs e)
        {

            var srcAddr = "0.0.0.0";
            var ipInfo = GetCurrentIpAddrInfo();
            if (ipInfo != null)
            {
                srcAddr = ipInfo.Address?.ToString();
            }


            int fps = (int)fpsNumeric.Value;


            bool showMouse = showMouseCheckBox.Checked;
            bool enableInputSimulator = inputSimulatorCheckBox.Checked;

            bool aspectRatio = false;

            var srcRect = currentScreen.Bounds;

            var destSize = new Size(srcRect.Width, srcRect.Height);
            if (aspectRatio)
            {
                var ratio = srcRect.Width / (double)srcRect.Height;
                int destWidth = destSize.Width;
                int destHeight = (int)(destWidth / ratio);
                if (ratio < 1)
                {
                    destHeight = destSize.Height;
                    destWidth = (int)(destHeight * ratio);
                }

                destSize = new Size(destWidth, destHeight);
            }

            screenSource = new ScreenSource();
            ScreenCaptureParams captureParams = new ScreenCaptureParams
            {
                SrcRect = srcRect,
                DestSize = destSize,
                CaptureType = CaptureType.DXGIDeskDupl,
                //CaptureType = CaptureType.Direct3D,
                //CaptureType = CaptureType.GDI,
                Fps = fps,
                CaptureMouse = showMouse,
            };

            screenSource.Setup(captureParams);

            var cmdOptions = new CommandLineOptions();
            cmdOptions.IpAddr = addressTextBox.Text;
            cmdOptions.Port = (int)portNumeric.Value;

            NetworkStreamingParams networkParams = new NetworkStreamingParams
            {

                SrcAddr = srcAddr,

                DestAddr = cmdOptions.IpAddr,
                DestPort = cmdOptions.Port,
            };

            VideoEncodingParams encodingParams = new VideoEncodingParams
            {
                Width = destSize.Width, // options.Width,
                Height = destSize.Height, // options.Height,
                FrameRate = cmdOptions.FrameRate,
                EncoderName = "libx264", // "h264_nvenc", //
            };

            videoStreamer = new ScreenStreamer(screenSource);
            videoStreamer.Setup(encodingParams, networkParams);



            statisticForm.Location = currentScreen.Bounds.Location;
            statisticForm.Start();

            var captureTask = screenSource.Start();
            var streamerTask = videoStreamer.Start();

            //previewForm = new PreviewForm();
            //previewForm.Setup(screenSource);

            if (enableInputSimulator)
            {
                desktopMan = new DesktopManager();
                var inputSimulatorTask = desktopMan.Start();
            }

            isStreaming = true;

            UpdateControls();

        }


        private void stopButton_Click(object sender, EventArgs e)
        {
            if (screenSource != null)
            {
                screenSource.Close();
            }

            if (videoStreamer != null)
            {
                videoStreamer.Close();

            }

            if (statisticForm != null)
            {
                statisticForm.Stop();
                statisticForm.Visible = false;
            }

            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Close();
                previewForm = null;
            }

            if (desktopMan != null)
            {
                desktopMan.Stop();
                desktopMan = null;
            }

            isStreaming = false;

            UpdateControls();



        }

        private void UpdateControls()
        {
            this.settingPanel.Enabled = !isStreaming;

            this.startButton.Enabled = !isStreaming;
            this.previewButton.Enabled = isStreaming;

            this.stopButton.Enabled = isStreaming;

            this.startRemoteServButton.Enabled = !ServiceHostOpened;
            this.stopRemoteServButton.Enabled = ServiceHostOpened;
        }

        private Screen currentScreen = null;
        private void screensComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            var obj = screensComboBox.SelectedItem;
            if (obj != null)
            {
                var item = obj as ComboBoxItem;
                if (item != null)
                {
                    var tag = item.Tag;
                    if(tag != null)
                    {
                        currentScreen = tag as Screen;
                    }
                }
            }
        }

        private StatisticForm statisticForm = new StatisticForm();
        private PreviewForm previewForm = null;

        private void previewButton_Click(object sender, EventArgs e)
        {
            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Visible = !previewForm.Visible;
            }
            else
            {

                previewForm = new PreviewForm();
                previewForm.Setup(screenSource);
                previewForm.Visible = true;
            }
        }

        RemoteDesktopService desktopEngine = null;

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
            desktopEngine = new RemoteDesktopService();

         
            //var address = @"net.tcp://localhost/RemoteDesktop";
            var _addr = textBox1.Text;

            var address = "net.tcp://" + _addr + "/RemoteDesktop";

            desktopEngine.Open(address);


            UpdateControls();

        }

        private void stopRemoteServButton_Click(object sender, EventArgs e)
        {
            logger.Debug("stopRemoteServButton_Click(...)");

            

            if (desktopEngine != null)
            {
                desktopEngine.Close();
                desktopEngine = null;
            }

            UpdateControls();
        }


        public static readonly string CurrentDirectory = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName;
        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(CurrentDirectory, "TestClient.exe"));
        }

        private void networkComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            var addr = "0.0.0.0";
            var ipInfo = GetCurrentIpAddrInfo();

            if (ipInfo != null)
            {
                addr = ipInfo.Address.ToString();
            }

            textBox1.Text = addr;

        }

        public IPAddressInformation GetCurrentIpAddrInfo()
        {
            IPAddressInformation ipInfo = null;

            var obj = networkComboBox.SelectedItem;
            if (obj != null)
            {
                var item = obj as ComboBoxItem;
                if (item != null)
                {
                    var tag = item.Tag;
                    if (tag != null)
                    {
                        ipInfo = tag as IPAddressInformation;
                    }
                }
            }

            return ipInfo;
        }

        class ComboBoxItem
        {
            public string Name { get; set; }
            public object Tag { get; set; }
        }

        private static List<ComboBoxItem> GetNetworkItems()
        {

            List<ComboBoxItem> networkItems = new List<ComboBoxItem>();

            var networkAny = new ComboBoxItem
            {
                Name = "_Any",
                Tag = null,
            };
            networkItems.Add(networkAny);

            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {

                if (network.OperationalStatus == OperationalStatus.Up &&
                    network.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {

                    IPInterfaceProperties prop = network.GetIPProperties();

                    foreach (IPAddressInformation addr in prop.UnicastAddresses)
                    {
                        //var phAddr = network.GetPhysicalAddress();
                        //logger.Debug(addr.Address.ToString() + " (" + network.Name + " - " + network.Description + ") " + phAddr.ToString());

                        // IPv4
                        if (addr.Address.AddressFamily != AddressFamily.InterNetwork)
                        {
                            continue;
                        }

                        networkItems.Add(new ComboBoxItem
                        {
                            Name = network.Name + " (" + addr.Address.ToString() + ")",
                            Tag = addr,
                        });

                    }
                }
            }

            return networkItems;
        }





        private void updateNetworksButton_Click(object sender, EventArgs e)
        {
            UpdateNetworks();

        }

        private void UpdateNetworks()
        {
            var networks = GetNetworkItems();

    
            networkComboBox.DataSource = networks;
            networkComboBox.DisplayMember = "Name";

            var maxWidth = DropDownWidth(networkComboBox);
            networkComboBox.DropDownWidth = maxWidth;
        }


        int DropDownWidth(ComboBox comboBox)
        {
            int maxWidth = 0, temp = 0;
            foreach (var obj in comboBox.Items)
            {
                temp = TextRenderer.MeasureText(comboBox.GetItemText(obj), comboBox.Font).Width;
                if (temp > maxWidth)
                {
                    maxWidth = temp;
                }
            }
            return maxWidth;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //this.notifyIcon.Visible = true;
            //this.notifyIcon.BalloonTipTitle = "TEST_TITLE";
            //this.notifyIcon.BalloonTipText = "TEST_TEXT";
            ////this.notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            ///
            this.notifyIcon.ShowBalloonTip(10000, "TEST_TITLE", "TEST_TEXT", ToolTipIcon.Info);
        }
    }


}
