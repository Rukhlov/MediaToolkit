using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaToolkit.UI
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
            this.labelStats.Text = GetReport();//Statistic.GetReport();

        }

        private List<StatCounter> statCounters = new List<StatCounter>();

        public string GetReport()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var stat in statCounters)
            {
                if (stat != null)
                {
                    sb.AppendLine(stat.GetReport());
                }
                
            }

            return sb.ToString();
        }

        public void Start(IEnumerable<StatCounter> stats)
        {

            statCounters.Clear();
            statCounters.AddRange(stats);

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
				cp.ExStyle |= NativeAPIs.WS_EX.TopMost;//0x00000008; //WS_EX_TOPMOST

				cp.ExStyle |= NativeAPIs.WS_EX.NoActivate; //WS_EX_NOACTIVATE
                cp.ExStyle |= NativeAPIs.WS_EX.Transparent; //WS_EX_TRANSPARENT
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
