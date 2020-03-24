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

namespace MediaToolkit.UI
{
    public partial class PreviewForm : Form
    {
        public PreviewForm()
        {
            InitializeComponent();
        }

        public void Link(D3DImageProvider provider)
        {
            d3DImageControl1.DataContext = provider;
        }

        public void UnLink()
        {
            d3DImageControl1.DataContext = null;
        }
    }
}
