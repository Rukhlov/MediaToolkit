using MediaToolkit;
using MediaToolkit.Common;
using MediaToolkit.Core;
using NAudio.CoreAudioApi;
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


            LoadNetworks();

            screenStreamerControl.Link(this);

        }

        private bool allowshowdisplay = true;

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(allowshowdisplay ? value : allowshowdisplay);
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

            //textBox1.Text = addr;

        }

        public int CommunicationPort
        {
            get
            {
                return (int)communicationPortNumeric.Value;
            }
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
            LoadNetworks();

        }

        private void LoadNetworks()
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




        private void multicastAddressTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void screenStreamerControl_Load(object sender, EventArgs e)
        {

        }

        private void remoteServerControl1_Load(object sender, EventArgs e)
        {

        }
    }


    class RegionForm : Form
    {
        internal RegionForm(Rectangle region)
        {
            this.StartPosition = FormStartPosition.Manual;
            this.Location = region.Location;
            this.Size = new Size(region.Width, region.Height);

            this.TransparencyKey = Color.White;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            RegionPanel panel = new RegionPanel();
            panel.Dock = DockStyle.Fill;

            this.Controls.Add(panel);
        }

        const int WS_EX_LAYERED = 0x00080000;
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams createParams = base.CreateParams;
        //        createParams.ExStyle |= WS_EX_LAYERED;
        //        return createParams;
        //    }
        //}

        class RegionPanel : Panel
        {
            internal RegionPanel()
            {
                timer.Tick += Timer_Tick;
                timer.Interval = 1000;
                timer.Enabled = true;

            }


            private byte tick = 0;
            private Timer timer = new Timer();
            private void Timer_Tick(object sender, EventArgs e)
            {
                DrawBorder();

                tick++;


            }

            private void DrawBorder()
            {
                var color = Color.Red;
                var color2 = Color.Green;

                if (tick % 2 == 0)
                {
                    color = Color.Green;
                    color2 = Color.Red;
                }

                var r = this.ClientRectangle;
                var rect = new Rectangle(r.X, r.Y, r.Width - 1, r.Height - 1);
                var g = Graphics.FromHwnd(this.Handle);

                using (var b = new SolidBrush(color))
                {
                    using (var pen = new Pen(b, 3))
                    {
                        g.DrawRectangle(pen, rect);
                    }
                }

                using (var b = new SolidBrush(color2))
                {
                    using (var pen = new Pen(b, 3))
                    {
                        pen.DashPattern = new float[] { 5, 5 };
  
                        g.DrawRectangle(pen, rect);
                    }
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                DrawBorder();

                base.OnPaint(e);
            }

            protected override void Dispose(bool disposing)
            {

                timer.Tick -= Timer_Tick;
                timer.Dispose();

                base.Dispose(disposing);
            }

        }
    }

    class ComboBoxItem
    {
        public string Name { get; set; }
        public object Tag { get; set; }
    }


}
