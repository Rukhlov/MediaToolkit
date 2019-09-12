using MediaToolkit;
using MediaToolkit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestStreamer.Controls
{
    public partial class PreviewForm : Form
    {
        public PreviewForm()
        {
            InitializeComponent();
        }

        private D3DImageProvider provider = null;
        public void Setup(ScreenSource source)
        {

            provider = new D3DImageProvider();
            provider.Setup(source);

            d3DImageControl1.DataContext = provider;
        }
    }
}
