using ScreenStreamer.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenStreamer.WinForms.App.Controls
{
    public partial class StreamInfoForm : Form
    {
        public StreamInfoForm()
        {
            InitializeComponent();

        }

        private Timer timer = new Timer();

        private StreamSession streamSession = null;
        public void Setup(StreamSession session)
        {
            this.streamSession = session;

            UpdateInfo();

            timer.Tick += Timer_Tick;
            timer.Interval = 300;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateInfo();

        }

        private void UpdateInfo()
        {
            if (streamSession != null)
            {
                this.Text = streamSession.StreamName;
                //streamSession.CommunicationAddress + ":" + streamSession.CommunicationPort;
            }
        }
    }
}
