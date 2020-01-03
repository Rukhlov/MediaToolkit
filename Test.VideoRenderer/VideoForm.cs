using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.VideoRenderer
{
    public partial class VideoForm : Form
    {
        public VideoForm()
        {
            InitializeComponent();

            //this.SetStyle(ControlStyles.ResizeRedraw, false);
            //this.SetStyle(ControlStyles.Opaque, true);
            //this.SetStyle(ControlStyles.UserPaint, true);
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //this.DoubleBuffered = false;

        }

        //protected override void OnPaintBackground(PaintEventArgs pevent)
        //{
        //    //...
        //}

        protected override void WndProc(ref Message m)
        {
            //System.Diagnostics.Debug.WriteLine(m.ToString());

            base.WndProc(ref m);
        }
        protected override void OnResize(EventArgs e)
        {

            base.OnResize(e);
        }


    }
}
