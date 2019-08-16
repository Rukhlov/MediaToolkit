using ScreenStreamer.Utils;
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
            //timer.Elapsed+= Timer_Elapsed;
            timer.Tick += Timer_Tick;
            timer.Interval = 100;
        }


        //private System.Timers.Timer timer = new System.Timers.Timer();

        private Timer timer = new Timer();
        private void Timer_Tick(object sender, EventArgs e)
        {

            this.labelCpuUsage.Text = Statistic.PerfCounter.GetReport();
            this.labelStats.Text = Statistic.GetReport();

        }

        public void Start()
        {

            timer.Enabled = true;

            this.Visible = true;
            //this.ShowDialog();
        }

        public void Stop()
        {
            timer.Enabled = false;
            Reset();
        }

        public void Reset()
        {
            //this.labelFps.Text = "--- FPS";
            //this.labelFramesCount.Text = "--- Frames";
            //this.labelBytesPerSec.Text = "--- KByte/s";
            //this.labelCaptureStats.Text = "--- MByte";
            //this.labelTime.Text = "";

            //this.labelNetworkStats.Text = "--- Packets";
            //this.labelRtpTotal.Text = "--- MBytes";
            //this.labelRtpBytesPerSec.Text = "--- MByte/s";

        }


        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000008; //WS_EX_TOPMOST
                cp.ExStyle |= 0x80; //WS_EX_NOACTIVATE
                cp.ExStyle |= 0x00000020; //WS_EX_TRANSPARENT
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
