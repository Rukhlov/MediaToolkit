using MediaToolkit.SharedTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.PolywallClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public Form1(Control control) :this()
        {
            control.Dock = DockStyle.Fill;

            this.panel1.Controls.Add(control);
        }

        private void button1_Click(object sender, EventArgs e)
        {



        }

        private IScreenCasterControl screenCasterControl = null;
        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start(@"C:\Users\Alexander\Source\Repos\ScreenStreamer\bin\Debug\Test.Streamer.exe");
        }


    }
}
