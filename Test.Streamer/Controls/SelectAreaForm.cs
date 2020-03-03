using MediaToolkit.Core;
using MediaToolkit.NativeAPIs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.Streamer.Controls
{
    public partial class SelectAreaForm : Form
    {

        public SelectAreaForm(bool debugMode = false)
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.DebugMode = debugMode;

            //this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            //this.SetStyle(ControlStyles.ResizeRedraw, false);
            //this.SetStyle(ControlStyles.Opaque, false);
            //this.SetStyle(ControlStyles.UserPaint, true);
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            this.labelInfo.Location = new Point(FrameWidth, FrameWidth);
            this.buttonClose.Location = new Point(this.Width - (buttonClose.Width + FrameWidth), FrameWidth);

            framePen1.Width = FrameWidth;

            framePen2.DashPattern = new float[] { 7, 7 };
            framePen2.Width = FrameWidth;

            buttonClose.Visible = DebugMode;
            labelInfo.Visible = DebugMode;

            panelMove.Visible = !capturing;


        }

        public ScreenCaptureDeviceDescription  CaptureDeviceDescription { get; set; }

        public bool DebugMode { get; set; } = false;
        private bool capturing = false;

        public bool Capturing
        {
            get
            {
                return capturing;
            }
            set
            {
                if(capturing != value )
                {
                    capturing = value;

                    panelMove.Visible = !capturing;
                    this.Refresh();
                }
            }
        } 

        //private Pen framePen1 = new Pen(Color.Red);

        // private Pen framePen1 = new Pen(Color.WhiteSmoke);
        //// private Pen framePen2 = new Pen(Color.FromArgb(46, 131, 241));

        // //private Pen framePen1 = new Pen(Color.FromArgb(46, 131, 241));
        // private Pen framePen2 = new Pen(Color.FromArgb(255, 127, 86));

        private Pen framePen1 = new Pen(Color.FromArgb(46, 131, 241));
        //private Pen framePen1 = new Pen(Color.FromArgb(255, 127, 86));
        private Pen framePen2 = new Pen(Color.WhiteSmoke);


        public Color FrameColor { get; set; } = Color.FromArgb(255, 127, 86);//Color.Red;


        public int FrameWidth { get; set; } = 4;

        private Rectangle TopRect
        {// вехняя граница
            get
            {
                return new Rectangle(0, 0, ClientSize.Width, FrameWidth);
            }
        }

        private Rectangle TopLeftRect
        {// левый верхний угол
            get
            {
                return new Rectangle(0, 0, FrameWidth, FrameWidth);
            }
        }

        private Rectangle LeftRect
        {
            get
            {
                return new Rectangle(0, 0, FrameWidth, ClientSize.Height);
            }
        }

        private Rectangle BottomLeftRect
        {
            get
            {
                return new Rectangle(0, ClientSize.Height - FrameWidth, FrameWidth, FrameWidth);
            }
        }

        private Rectangle BottomRect
        {
            get
            {
                return new Rectangle(0, ClientSize.Height - FrameWidth, ClientSize.Width, FrameWidth);
            }
        }

        private Rectangle BottomRightRect
        {
            get
            {
                return new Rectangle(ClientSize.Width - FrameWidth, ClientSize.Height - FrameWidth, FrameWidth, FrameWidth);
            }
        }

        private Rectangle RightRect
        {
            get
            {
                return new Rectangle(ClientSize.Width - FrameWidth, 0, FrameWidth, ClientSize.Height);
            }
        }

        private Rectangle TopRightRect
        {
            get
            {
                return new Rectangle(ClientSize.Width - FrameWidth, 0, FrameWidth, FrameWidth);
            }
        }


        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style &= ~(WS.MaximizeBox | WS.Overlapped);

                cp.ExStyle |= WS_EX.Composited;
                cp.ExStyle |= WS_EX.TopMost;
                cp.ExStyle |= WS_EX.ToolWindow;

                // cp.ExStyle |= WS_EX.Layered;
                //cp.ExStyle |= 0x80; //WS_EX_NOACTIVATE
                //cp.ExStyle |= 0x00000020; //WS_EX_TRANSPARENT
                return cp;
            }
        }


        protected override void OnResizeBegin(EventArgs e)
        {
            //SuspendLayout();
            base.OnResizeBegin(e);
        }
        protected override void OnResizeEnd(EventArgs e)
        {

            //ResumeLayout(true);
            base.OnResizeEnd(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

        }

        protected override void OnPaint(PaintEventArgs e) 
        {
            var g = e.Graphics;
            //using (Brush b = new SolidBrush(FrameColor))
            //{
            //    g.FillRectangle(b, TopRect);
            //    g.FillRectangle(b, LeftRect);
            //    g.FillRectangle(b, RightRect);
            //    g.FillRectangle(b, BottomRect);
            //}

            //

            //var leftTop = new Point(0, 0);
            //var leftBottom = new Point(0, this.ClientSize.Height - FrameWidth /2);
            //var rightTop = new Point(this.ClientSize.Width - FrameWidth/2, 0);
            //var rightBottom = new Point(this.ClientSize.Width - FrameWidth/2, this.ClientSize.Height - FrameWidth/2);


            //g.DrawLine(framePen1, leftTop, rightTop);
            //g.DrawLine(framePen1, rightTop, rightBottom);
            //g.DrawLine(framePen1, leftBottom, rightBottom);
            //g.DrawLine(framePen1, leftBottom, leftTop);

            // g.DrawRectangle(new Pen(Brushes.Red, 5), rect);


            var x = this.ClientRectangle.X+ FrameWidth;
            var y = this.ClientRectangle.Y+ FrameWidth;
            var width = (this.ClientRectangle.Width - 2 * FrameWidth);
            var height = (this.ClientRectangle.Height - 2*FrameWidth);
            var rect = new Rectangle(x, y, width, height);


            //g.DrawRectangle(new Pen(FrameColor, 4), rect);

            g.DrawRectangle(framePen1, rect);
            g.DrawRectangle(framePen2, rect);

            if (!capturing)
            {
                g.DrawLine(framePen1, new Point(x + 6, y + 8), new Point(x + 28, y + 8));
                g.DrawLine(framePen1, new Point(x + 8, y + 8), new Point(x + 8, y + 28));

                int centerX = (width - x) / 2 + 6;

                g.DrawLine(framePen1, new Point(centerX - 12, y + 8), new Point(centerX + 12, y + 8));


                g.DrawLine(framePen1, new Point(width - 4, y + 8), new Point(width - 24, y + 8));
                g.DrawLine(framePen1, new Point(width - 4, y + 6), new Point(width - 4, y + 28));

                int centerY = (height - y) / 2 + 6;

                g.DrawLine(framePen1, new Point(width - 4, centerY - 12), new Point(width - 4, centerY + 12));


                g.DrawLine(framePen1, new Point(width - 4, height - 4), new Point(width - 24, height - 4));
                g.DrawLine(framePen1, new Point(width - 4, height - 2), new Point(width - 4, height - 24));


                g.DrawLine(framePen1, new Point(centerX - 12, height - 4), new Point(centerX + 12, height - 4));


                g.DrawLine(framePen1, new Point(x + 6, height - 4), new Point(x + 28, height - 4));
                g.DrawLine(framePen1, new Point(x + 8, height - 2), new Point(x + 8, height - 24));


                g.DrawLine(framePen1, new Point(x + 8, centerY - 12), new Point(x + 8, centerY + 12));
            }


        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {

            base.OnPaintBackground(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            var rect = new Rectangle(this.Location, this.Size);
            this.labelInfo.Text = rect.ToString();

            UpdateDeviceDescr(rect);

        }



        internal void UpdateDeviceDescr(Rectangle rect)
        {
            if (CaptureDeviceDescription != null)
            {
                CaptureDeviceDescription.DisplayRegion = rect;
                CaptureDeviceDescription.CaptureRegion = rect;
                CaptureDeviceDescription.Resolution = rect.Size;
            }
        }

        protected override void OnLocationChanged(EventArgs e)
        {

            base.OnLocationChanged(e);

            var rect = new Rectangle(this.Location, this.Size);
            this.labelInfo.Text = rect.ToString();

            UpdateDeviceDescr(rect);


        }

        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);

            if (!capturing)
            {
                if (message.Msg == WM.NCHITTEST)
                {
                    var cursorPosition = this.PointToClient(Cursor.Position);
                    message.Result = (IntPtr)HitTestFrame(cursorPosition);

                }

            }

            //Debug.WriteLine(message.ToString());
        }

        private Rectangle prevRect = Rectangle.Empty;
        private void panelMove_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var virtualScreen = SystemInformation.VirtualScreen;

            this.Visible = false;
            this.SuspendLayout();

            if (prevRect.IsEmpty)
            {
                prevRect = new Rectangle(this.Location, this.Size);

                var x = virtualScreen.Location.X - FrameWidth;
                var y = virtualScreen.Location.Y - FrameWidth;
                var width = virtualScreen.Size.Width + 2 * FrameWidth;
                var height = virtualScreen.Size.Height + 2 * FrameWidth;

                this.Location = new Point(x, y);
                this.Size = new Size(width, height);

            }
            else
            {
                this.Location = prevRect.Location;
                this.Size = prevRect.Size;
                prevRect = Rectangle.Empty;
            }

            this.ResumeLayout(true);

            this.Visible = true;

        }

        private void panelMove_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Clicks == 1)
                {
                    User32.ReleaseCapture();
                    User32.SendMessage(Handle, WM.NCLBUTTONDOWN, WM.HTCAPTION, 0);
                }

            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private int HitTestFrame(Point point)
        {
            int result = 0;

            if (TopLeftRect.Contains(point))
            {
                result = WM.HTTOPLEFT;
            }
            else if (TopRightRect.Contains(point))
            {
                result = WM.HTTOPRIGHT;
            }
            else if (BottomLeftRect.Contains(point))
            {
                result = WM.HTBOTTOMLEFT;
            }
            else if (BottomRightRect.Contains(point))
            {
                result = WM.HTBOTTOMRIGHT;
            }
            else if (TopRect.Contains(point))
            {
                result = WM.HTTOP;
            }
            else if (LeftRect.Contains(point))
            {
                result = WM.HTLEFT;
            }
            else if (RightRect.Contains(point))
            {
                result = WM.HTRIGHT;
            }
            else if (BottomRect.Contains(point))
            {
                result = WM.HTBOTTOM;
            }

            return result;
        }


    }

}
