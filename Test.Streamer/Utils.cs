using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace TestStreamer
{

    public class SnippingTool
    {
        private SnippingForm form;

        private Action<Rectangle, Rectangle> AreaSelected;

        public void Snip(Screen screen, Action<Rectangle, Rectangle> areaSelected)
        {
            var bound = screen?.Bounds ?? SystemInformation.VirtualScreen;
            Snip(bound, areaSelected);

        }
        public void Snip(Rectangle bound, Action<Rectangle, Rectangle> areaSelected)
        {

            if (form != null && !form.IsDisposed && !form.Disposing)
            {
                form.Dispose();
            }

            this.AreaSelected = areaSelected;



            var bmp = new Bitmap(bound.Width, bound.Height, PixelFormat.Format32bppPArgb);
            try
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(bound.X, bound.Y, 0, 0, bmp.Size);//, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
                }

                form = new SnippingForm(this)
                {
                    BackgroundImage = bmp,
                    Bounds = bound,
                };

                form.Show();
            }
            catch (Exception ex)
            {
                bmp?.Dispose();
                form?.Dispose();

                AreaSelected = null;
            }
        }



        public void Dispose()
        {
            if (form != null)
            {
                form.Dispose();
                form = null;
            }

            this.AreaSelected = null;
        }

        public class SnippingForm : Form
        {

            public SnippingForm(SnippingTool tool)
            {
                this.tool = tool;

                BackgroundImageLayout = ImageLayout.Stretch;
                ShowInTaskbar = false;
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;

                DoubleBuffered = true;
                //var cur = Properties.Resources.cross_128x128;
                //using (var stream = new MemoryStream(cur))
                //{
                //    Cursor = new System.Windows.Forms.Cursor(stream);
                //}
                
                Cursor = Cursors.Cross;
            
                TopMost = true;
            }

            private readonly SnippingTool tool;
            private Rectangle selectionRect;
            private Point startPoint;

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left)
                {
                    return;
                }

                startPoint = e.Location;
                selectionRect = new Rectangle(startPoint, new Size(0, 0));
                Invalidate();
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left)
                {
                    return;
                }

                int x1 = Math.Min(e.X, startPoint.X);
                int y1 = Math.Min(e.Y, startPoint.Y);
                int x2 = Math.Max(e.X, startPoint.X);
                int y2 = Math.Max(e.Y, startPoint.Y);

                int right2 = this.Width;
                if (x2 > right2)
                {
                    x2 = right2;
                }

                int bottom2 = this.Height;
                if (y2 > bottom2)
                {
                    y2 = bottom2;
                }

                int w = x2 - x1;
                int h = y2 - y1;

                selectionRect = new Rectangle(x1, y1, w, h);

                Invalidate();
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {

                var hScale = BackgroundImage.Width / (double)Width;
                var vScale = BackgroundImage.Height / (double)Height;

                var srcRect = new Rectangle((int)(selectionRect.X * hScale), (int)(selectionRect.Y * vScale),
                    (int)(selectionRect.Width * hScale), (int)(selectionRect.Height * vScale));

                var screenRect = new Rectangle(this.Location, this.Size);

                tool.AreaSelected?.Invoke(srcRect, screenRect);

                this.Close();
            }

            protected override void OnPaint(PaintEventArgs e)
            {

                using (Brush br = new SolidBrush(Color.FromArgb(120, Color.White)))
                {
                    int x1 = selectionRect.X;
                    int x2 = selectionRect.X + selectionRect.Width;
                    int y1 = selectionRect.Y;
                    int y2 = selectionRect.Y + selectionRect.Height;

                    e.Graphics.FillRectangle(br, new Rectangle(0, 0, x1, Height));
                    e.Graphics.FillRectangle(br, new Rectangle(x2, 0, Width - x2, Height));
                    e.Graphics.FillRectangle(br, new Rectangle(x1, 0, x2 - x1, y1));
                    e.Graphics.FillRectangle(br, new Rectangle(x1, y2, x2 - x1, Height - y2));
                }
                using (Pen pen = new Pen(Color.Red, 1))
                {
                    e.Graphics.DrawRectangle(pen, selectionRect);
                }
            }

            protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
            {
                if (keyData == Keys.Escape)
                {
                    this.Close();
                }

                return base.ProcessCmdKey(ref msg, keyData);
            }

            protected override void Dispose(bool disposing)
            {
                if (this.BackgroundImage != null)
                {
                    this.BackgroundImage.Dispose();
                }

                base.Dispose(disposing);
            }

        }

    }
}
