using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaToolkit.UI
{
    public class SnippingTool
    {

        public static void Snip(Screen screen)
        {
            //DPI_AWARE !!!
            var bound = screen?.Bounds ?? SystemInformation.VirtualScreen;
            Snip(bound);

        }

        public static Rectangle Snip(Rectangle bound)
        {
            if (bound.IsEmpty)
            {
                throw new ArgumentException("bound.IsEmpty");
            }

            Rectangle outputRegion = Rectangle.Empty;

            var bmp = new Bitmap(bound.Width, bound.Height, PixelFormat.Format32bppPArgb);
            try
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(bound.X, bound.Y, 0, 0, bmp.Size);//, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
                }

                var form = new SnippingForm()
                {
                    BackgroundImage = bmp,
                    Bounds = bound,
                };

                try
                {
                    //form.MouseUp += (o, a) =>
                    //{
                    //    form.Close();
                    //};

                    form.ShowDialog();

                    var srcRect = form.SourceRectangle;
                    var screenRect = form.ScreenRectangle;


                    int left = srcRect.Left + screenRect.Left;
                    int top = srcRect.Top + screenRect.Top;

                    outputRegion = new Rectangle(left, top, srcRect.Width, srcRect.Height);
                }
                finally
                {
                    form?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
                Trace.WriteLine(ex);
            }
            finally
            {
                bmp?.Dispose();
            }

            return outputRegion;
        }


        class SnippingForm : Form
        {

            public SnippingForm()
            {

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

            private Rectangle selectionRect;
            private Point startPoint;

            public Rectangle ScreenRectangle { get; private set; } = Rectangle.Empty;
            public Rectangle SourceRectangle { get; private set; } = Rectangle.Empty;


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

                int x = (int)(selectionRect.X * hScale);
                int y = (int)(selectionRect.Y * vScale);
                int w = (int)(selectionRect.Width * hScale);
                int h = (int)(selectionRect.Height * vScale);

                this.SourceRectangle = new Rectangle(x, y, w, h);
                this.ScreenRectangle = new Rectangle(this.Location, this.Size);

               // base.OnMouseUp(e);

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
