using MediaToolkit.Core;
using MediaToolkit.NativeAPIs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
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

            //TextureBrush brush = new TextureBrush(Properties.Resources.test);

            //framePen1 = new Pen(brush);


        }

        private Pen testPen = new Pen(new TextureBrush(Properties.Resources.test));

        public ScreenCaptureDeviceDescription CaptureDeviceDescription { get; set; }

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
                if (capturing != value)
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


        public int FrameWidth { get; set; } = 5;

        public int GripWidth { get; set; } = 25;
        public int GripHeight { get; set; } = 25;
        public int GripIdent { get; set; } = 5;


        private Rectangle TopLine
        {// вехняя граница
            get
            {
                return new Rectangle(0, 0, ClientSize.Width, FrameWidth);
            }
        }

        private Rectangle TopLeftGrip
        {// левый верхний угол
            get
            {
                int x = GripIdent;
                int y = GripIdent;

                int w = GripIdent + GripWidth + FrameWidth / 2;
                int h = GripIdent + GripHeight + FrameWidth / 2;

                return new Rectangle(x, y, w, h);

                //return new Rectangle(0, 0, FrameWidth, FrameWidth);
            }
        }

        private Rectangle TopGrip
        {// вехняя граница
            get
            {

                var width = (this.ClientRectangle.Width - 2 * FrameWidth);

                int x =(int)((width ) / 2f - (GripIdent + (FrameWidth )));
                int y = GripIdent;

                int w = GripIdent + GripWidth + FrameWidth / 2;
                int h = GripIdent + 2 * FrameWidth;

                return new Rectangle(x, y, w, h);

                //return new Rectangle(0, 0, ClientSize.Width, FrameWidth);
            }
        }

        private Rectangle LeftLine
        {
            get
            {
                return new Rectangle(0, 0, FrameWidth, ClientSize.Height);
            }
        }

        private Rectangle LeftGrip
        {
            get
            {
                var height = (this.ClientRectangle.Height - 2 * FrameWidth);

                int x = GripIdent;
                //int y = ClientSize.Height - (GripHeight + GripIdent + FrameWidth);

                int y = (int)(height / 2f - (GripIdent + (FrameWidth)));

                int w = GripIdent + 2 * FrameWidth;
                int h = GripIdent + GripHeight + FrameWidth / 2;

                return new Rectangle(x, y, w, h);
                //return new Rectangle(0, 0, FrameWidth, ClientSize.Height);
            }
        }



        private Rectangle BottomLeftGrip
        {
            get
            {
                int x = GripIdent;
                int y = ClientSize.Height - (GripHeight + GripIdent + FrameWidth);

                int w = GripIdent + GripWidth + FrameWidth / 2;
                int h = GripIdent + GripHeight + FrameWidth / 2;

                return new Rectangle(x, y, w, h);

                //return new Rectangle(0, ClientSize.Height - FrameWidth, FrameWidth, FrameWidth);
            }
        }



        private Rectangle BottomLine
        {
            get
            {
                return new Rectangle(0, ClientSize.Height - FrameWidth, ClientSize.Width, FrameWidth);
            }
        }

        private Rectangle BottomGrip
        {
            get
            {
                var width = (this.ClientRectangle.Width - 2 * FrameWidth);

                int x = (int)((width) / 2f - (GripIdent + (FrameWidth)));
                int y = (ClientSize.Height - FrameWidth  )- ( GripIdent +  2 * FrameWidth);

                int w = GripIdent + GripWidth + FrameWidth / 2;
                int h = GripIdent + 2 * FrameWidth;

                return new Rectangle(x, y, w, h);

                //return new Rectangle(0, ClientSize.Height - FrameWidth, ClientSize.Width, FrameWidth);
            }
        }

        private Rectangle BottomRightGrip
        {
            get
            {
                int x = ClientSize.Width - (GripWidth + GripIdent + FrameWidth);
                int y = ClientSize.Height - (GripHeight + GripIdent + FrameWidth);

                int w = GripWidth + FrameWidth;
                int h = GripIdent + GripHeight + FrameWidth / 2;

                return new Rectangle(x, y, w, h);

                //return new Rectangle(ClientSize.Width - FrameWidth, ClientSize.Height - FrameWidth, FrameWidth, FrameWidth);
            }
        }

        private Rectangle RightLine
        {
            get
            {
                return new Rectangle(ClientSize.Width - FrameWidth, 0, FrameWidth, ClientSize.Height);
            }
        }

        private Rectangle RightGrip
        {
            get
            {
                var height = (this.ClientRectangle.Height - 2 * FrameWidth);

                int x = (ClientSize.Width- FrameWidth) - ( GripIdent + 2* FrameWidth);
                //int y = ClientSize.Height - (GripHeight + GripIdent + FrameWidth);

                int y = (int)(height / 2f - (GripIdent + (FrameWidth)));

                int w = GripIdent + 2* FrameWidth;
                int h = GripIdent + GripHeight + FrameWidth / 2;

                return new Rectangle(x, y, w, h);

                //return new Rectangle(ClientSize.Width - FrameWidth, 0, FrameWidth, ClientSize.Height);
            }
        }

        private Rectangle TopRightGrip
        {
            get
            {
                int x = ClientSize.Width - (GripWidth + GripIdent +  FrameWidth);
                int y =  GripIdent;

                int w = GripWidth + FrameWidth;
                int h = GripIdent + GripHeight + FrameWidth / 2;

                return new Rectangle(x, y, w, h);

                //return new Rectangle(ClientSize.Width - FrameWidth, 0, FrameWidth, FrameWidth);
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

            //TextureBrush b = new TextureBrush(Properties.Resources.test);
            //g.FillRectangle(b, e.ClipRectangle);

            //return;


            //using (Brush b = new SolidBrush(FrameColor))
            //{
            //    //TextureBrush b = new TextureBrush(Properties.Resources.test);
            //    g.FillRectangle(b, TopLeftGrip);
            //    g.FillRectangle(b, TopRightGrip);
            //    g.FillRectangle(b, BottomRightGrip);
            //    g.FillRectangle(b, BottomLeftGrip);


            //    g.FillRectangle(b, TopGrip);
            //    g.FillRectangle(b, BottomGrip);

            //    g.FillRectangle(b, RightGrip);

            //    g.FillRectangle(b, LeftGrip);

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


            var x = this.ClientRectangle.X + FrameWidth;
            var y = this.ClientRectangle.Y + FrameWidth;
            var width = (this.ClientRectangle.Width - 2 * FrameWidth);
            var height = (this.ClientRectangle.Height - 2 * FrameWidth);

            var rect = new Rectangle(x, y, width, height);


            //g.DrawRectangle(new Pen(FrameColor, 4), rect);

            g.DrawRectangle(framePen1, rect);
            g.DrawRectangle(framePen2, rect);



            //var starPoint = new PointF(0.0f, 0.0f);
            //var endPoint = new PointF(20.0f, 20.0f);

            //var color2 = Color.FromArgb(46, 131, 241);
            //var color1 = Color.WhiteSmoke;

            //using (LinearGradientBrush b = new LinearGradientBrush(starPoint, endPoint, color1, color2))
            //{
            //    b.WrapMode = WrapMode.TileFlipXY;

            //    e.Graphics.FillRectangle(b, ClientRectangle);


            //    g.FillRectangle(new SolidBrush(Color.Black), rect);
            //}


            if (!capturing)
            {// draw frame grip
                //int gripIdent = FrameWidth;//4;
                //int gripWidth = FrameWidth * 5;//20;
                //int gripHeight = FrameWidth * 5;//20;

                {// top left
                    {
                        float x1 = (x + GripIdent + (FrameWidth / 2f));
                        float x2 = x1 + GripWidth;
                        float y1 = (y + GripIdent + FrameWidth);

                        g.DrawLine(framePen1, new PointF(x1, y1), new PointF(x2, y1));
                    }

                    {
                        float x1 = (x + GripIdent + FrameWidth);
                        float y1 = (y + GripIdent + FrameWidth / 2f);
                        float y2 = y1 + GripHeight;

                        g.DrawLine(framePen1, new PointF(x1, y1), new PointF(x1, y2));
                    }
                }


                // top center
                float centerX = (width - x) / 2f + (GripIdent + (FrameWidth / 2f));
                {
                    float x1 = centerX - GripWidth / 2f;
                    float x2 = centerX + GripWidth / 2f;
                    float y1 = (y + GripIdent + FrameWidth);
                    g.DrawLine(framePen1, new PointF(x1, y1), new PointF(x2, y1));
                }

                {// right top
                    {
                        float x1 = width - GripIdent;
                        float x2 = width - GripWidth - (GripIdent - (FrameWidth / 2f));
                        float y1 = (y + GripIdent + FrameWidth);

                        g.DrawLine(framePen1, new PointF(x1, y1), new PointF(x2, y1));
                    }

                    {
                        float x1 = width - GripIdent;

                        float y1 = (y + GripIdent + FrameWidth / 2f);
                        float y2 = y + GripHeight + GripIdent + FrameWidth / 2f;

                        g.DrawLine(framePen1, new PointF(x1, y1), new PointF(x1, y2));
                    }
                }


                // right center
                float centerY = (height - y) / 2f + (GripIdent + (FrameWidth / 2f));
                {
                    float x1 = width - GripIdent;
                    float y1 = centerY - GripHeight / 2f;
                    float y2 = centerY + GripHeight / 2f;

                    g.DrawLine(framePen1, new PointF(x1, y1), new PointF(x1, y2));

                }

                {// right bottom
                    {

                        float x1 = width - GripIdent;
                        float x2 = width - GripWidth - (GripIdent - (FrameWidth / 2f));
                        float y1 = height - GripIdent;

                        g.DrawLine(framePen1, new PointF(x1, y1), new PointF(x2, y1));
                    }

                    {
                        float x1 = width - GripIdent;
                        float y1 = height - (GripIdent - (FrameWidth / 2f));//gripIdent;
                        float y2 = height - GripHeight - (GripIdent - (FrameWidth / 2f));

                        g.DrawLine(framePen1, new PointF(x1, y1), new PointF(x1, y2));

                    }
                }



                {// bottom center

                    float x1 = centerX - GripWidth / 2f;
                    float x2 = centerX + GripWidth / 2f;
                    float y1 = height - GripIdent;
                    g.DrawLine(framePen1, new PointF(x1, y1), new PointF(x2, y1));
                }

                {// left bottom
                    {

                        float x1 = (x + GripIdent + (FrameWidth / 2f));
                        float x2 = x1 + GripWidth;
                        float y1 = height - GripIdent;

                        g.DrawLine(framePen1, new PointF(x1, y1), new PointF(x2, y1));
                    }

                    {

                        float x1 = (x + GripIdent + FrameWidth);
                        float y1 = height - (GripIdent - (FrameWidth / 2f));//gripIdent;
                        float y2 = height - GripHeight - (GripIdent - (FrameWidth / 2f));
                        g.DrawLine(framePen1, new PointF(x1, y1), new PointF(x1, y2));

                    }
                }


                {// left center

                    float x1 = (x + GripIdent + FrameWidth);
                    float y1 = centerY - GripHeight / 2f;
                    float y2 = centerY + GripHeight / 2f;

                    g.DrawLine(framePen1, new PointF(x1, y1), new PointF(x1, y2));
                }

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

            if (TopLeftGrip.Contains(point))
            {
                result = WM.HTTOPLEFT;
            }
            else if (TopRightGrip.Contains(point))
            {
                result = WM.HTTOPRIGHT;
            }
            else if (BottomLeftGrip.Contains(point))
            {
                result = WM.HTBOTTOMLEFT;
            }
            else if (BottomRightGrip.Contains(point))
            {
                result = WM.HTBOTTOMRIGHT;
            }
            else if (TopLine.Contains(point) || TopGrip.Contains(point))
            {
                result = WM.HTTOP;
            }
            else if (LeftLine.Contains(point) || LeftGrip.Contains(point))
            {
                result = WM.HTLEFT;
            }
            else if (RightLine.Contains(point) || RightGrip.Contains(point))
            {
                result = WM.HTRIGHT;
            }
            else if (BottomLine.Contains(point) || (BottomGrip.Contains(point)))
            {
                result = WM.HTBOTTOM;
            }

            return result;
        }


    }

}
