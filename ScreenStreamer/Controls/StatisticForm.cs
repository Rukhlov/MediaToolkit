using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenStreamer.Controls
{
    public partial class StatisticForm : Form
    {
        public StatisticForm()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(20, 20);
            this.ShowInTaskbar = false;
            this.TopMost = true;

            Reset();

            timer.Tick += Timer_Tick;
            timer.Interval = 100;
        }

        private Timer timer = new Timer();
        private void Timer_Tick(object sender, EventArgs e)
        {
            var stats = Statistic.CaptureStats;
            var perfCounter = Statistic.PerfCounter;
            var rtpStats = Statistic.RtpStats;

            {
                var fps = (1 / stats.avgFrameInterval);
                var mbytesPerSec = stats.avgBytesPerSec / (1024.0 * 1024);
                var mbytes = stats.totalBytes / (1024.0 * 1024);

                this.labelFps.Text = fps.ToString("0.0") + " FPS";
                this.labelFramesCount.Text = stats.totalFrameCount + " Frames";
                this.labelBytesPerSec.Text = mbytesPerSec.ToString("0.0") + " MByte/s";
                this.labelTotalBytes.Text = mbytes.ToString("0.0") + " MByte";
                this.labelTime.Text = DateTime.Now.ToString("HH:mm:ss.fff");

                this.labelCpuUsage.Text = perfCounter.GetCpuUsage();
            }
            {
                var mbytesPerSec = rtpStats.sendBytesPerSec / (1024.0 * 1024);
                var mbytes = rtpStats.bytesSend / (1024.0 * 1024);

                this.labelRtpCount.Text = rtpStats.packetsCount + " Packets";
                this.labelRtpTotal.Text = mbytes.ToString("0.0") + " MBytes";
                this.labelRtpBytesPerSec.Text = mbytesPerSec.ToString("0.000") + " MByte/s";
            }

        }

        public void Start()
        {

            timer.Enabled = true;
            this.Visible = true;
        }

        public void Stop()
        {
            timer.Enabled = false;
            Reset();
        }

        public void Reset()
        {
            this.labelFps.Text = "--- FPS";
            this.labelFramesCount.Text = "--- Frames";
            this.labelBytesPerSec.Text = "--- KByte/s";
            this.labelTotalBytes.Text = "--- MByte";
            this.labelTime.Text = "";

            this.labelRtpCount.Text = "--- Packets";
            this.labelRtpTotal.Text = "--- MBytes";
            this.labelRtpBytesPerSec.Text = "--- MByte/s";

        }


        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                cp.ExStyle |= 0x00000020;
                return cp;
            }
        }


        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }
    }
}
