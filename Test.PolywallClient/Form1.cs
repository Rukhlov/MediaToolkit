using MediaToolkit.SharedTypes;
using NLog;

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
        private static Logger logger = LogManager.GetCurrentClassLogger();

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

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Test.Streamer.exe");

                Process.Start(file);
            }
            catch(Exception ex)
            {

                logger.Error(ex);
            }

        }


    }
}
