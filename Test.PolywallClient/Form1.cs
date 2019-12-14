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

        private void button1_Click(object sender, EventArgs e)
        {
            var pluginPath = @"C:\Users\Alexander\Source\Repos\ScreenStreamer\bin\Debug";
            var fileFullName = pluginPath + @"\MediaToolkit.UI.dll";
            InstanceFactory.AssemblyPath = pluginPath;
            InstanceFactory.RegisterType<IScreenCasterControl>(fileFullName);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var screenCasterControl = InstanceFactory.CreateInstance<IScreenCasterControl>();

            if (screenCasterControl != null)
            {
                this.panel1.Controls.Add((Control)screenCasterControl);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start(@"C:\Users\Alexander\Source\Repos\ScreenStreamer\bin\Debug\Test.Streamer.exe");
        }
    }
}
