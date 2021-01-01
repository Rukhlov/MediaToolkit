
using ScreenStreamer.Wpf.Interfaces;
using ScreenStreamer.Wpf.ViewModels.Properties;
using System.Drawing;
using static ScreenStreamer.Wpf.Helpers.VideoHelper;
using ScreenStreamer.Wpf.Models;
using System.Drawing.Drawing2D;
using System;
using System.Diagnostics;
//using System.Windows;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using ScreenStreamer.Wpf.Utils;

namespace ScreenStreamer.Wpf.ViewModels
{
    public abstract class BaseBorderViewModel : Prism.Mvvm.BindableBase, IBorderViewModel
    {
        public StreamViewModel Stream { get; }
        private PropertyVideoViewModel videoViewModel = null;

        private PropertyBorderModel borderModel = null;
        public BaseBorderViewModel(StreamViewModel stream, PropertyBorderModel border)
        {
            this.Stream = stream;
            this.borderModel = border;

            videoViewModel = Stream.VideoViewModel;

            SetBorderRegion(borderModel.BorderRect);

            //Color color1 = Color.FromArgb(46, 131, 241);
            //Color color2 = Color.WhiteSmoke;

            //var bitmap = CreateStripesBitmap(new System.Drawing.Size(50, 50), color1, color2);

            //bitmapSource = null;

            //var hBitmap = bitmap.GetHbitmap();

            //try
            //{
            //    bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            //        hBitmap,
            //        IntPtr.Zero, System.Windows.Int32Rect.Empty,
            //        BitmapSizeOptions.FromEmptyOptions());
            //}
            //catch (Win32Exception)
            //{
            //    bitmapSource = null;
            //}
            //finally
            //{
            //    NativeMethods.DeleteObject(hBitmap);
            //}

        }

        //private BitmapSource bitmapSource = null;

        //public System.Windows.Media.ImageBrush StripesBrush
        //{
        //    get
        //    {
        //        System.Windows.Media.ImageBrush brush = new System.Windows.Media.ImageBrush();
        //        brush.ImageSource = bitmapSource;
        //        return brush;
        //    }
        //}

        private double wpfLeft = 0;
        public double WpfLeft
        {
            get
            {
                //return PixelsToPoints(videoViewModel.Left, LengthDirection.Horizontal);
                return wpfLeft;
            }
            set
            {
                wpfLeft = value;

                borderModel.Left = (int)PointsToPixels(wpfLeft, LengthDirection.Horizontal);

                if (videoViewModel != null)
                {
                    videoViewModel.Left = borderModel.Left;
                }

            }
        }

        private double wpfTop = 0;
        public double WpfTop
        {
            get
            {
                //return PixelsToPoints(videoViewModel.Top, LengthDirection.Vertical);
                return wpfTop;

            }
            set
            {

                wpfTop = value;

                borderModel.Top = (int)PointsToPixels(wpfTop, LengthDirection.Vertical);

                videoViewModel.Top = borderModel.Top;
                
            }
        }

        private double wpfWidth = 0;
        public double WpfWidth
        {
            get
            {
                // return PixelsToPoints(videoViewModel.ResolutionWidth, LengthDirection.Horizontal);
                return wpfWidth;
            }
            set
            {
                wpfWidth = value;
                borderModel.Width = (int)PointsToPixels(wpfWidth, LengthDirection.Horizontal);
                videoViewModel.ResolutionWidth = borderModel.Width;
            }
        }

        private double wpfHeight = 0;
        public double WpfHeight
        {
            get
            {
                //return PixelsToPoints(videoViewModel.ResolutionHeight, LengthDirection.Vertical);
                return wpfHeight;
            }
            set
            {

                wpfHeight = value;

                borderModel.Height = (int)PointsToPixels(wpfHeight, LengthDirection.Vertical);
                videoViewModel.ResolutionHeight = borderModel.Height;
            }
        }

        public void SetBorderRegion(Rectangle rect)
        {
           wpfLeft = PixelsToPoints(rect.X, LengthDirection.Horizontal);
           wpfTop= PixelsToPoints(rect.Y, LengthDirection.Vertical);
           wpfWidth = PixelsToPoints(rect.Width, LengthDirection.Horizontal);
           wpfHeight = PixelsToPoints(rect.Height, LengthDirection.Vertical);

        }

        public Rectangle GetScreenRegion()
        {
            var x = (int)PointsToPixels(WpfLeft, LengthDirection.Horizontal);
            var y = (int)PointsToPixels(WpfTop, LengthDirection.Vertical);
            var w = (int)PointsToPixels(WpfWidth, LengthDirection.Horizontal);
            var h = (int)PointsToPixels(WpfHeight, LengthDirection.Vertical);

            return new Rectangle(x, y, w, h);
        }


        private static Bitmap CreateStripesBitmap(System.Drawing.Size size, Color color1, Color color2)
        {
            int width = size.Width;
            int height = size.Height;

            Bitmap bmp = new Bitmap(width, height);
            try
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    var rect = new Rectangle(0, 0, width, height);
                    using (Brush b = new SolidBrush(color1))
                    {
                        g.FillRectangle(b, rect);
                    }

                    using (GraphicsPath graghPath = new GraphicsPath())
                    {
                        Point[] points =
                        {
                            new Point(0,0),
                            new Point(width/2,0),
                            new Point(0, height/2),

                        };

                        graghPath.AddLines(points);

                        using (Brush b = new SolidBrush(color2))
                        {
                            g.FillPath(b, graghPath);
                        }

                    }

                    using (GraphicsPath graghPath = new GraphicsPath())
                    {
                        Point[] points =
                        {
                            new Point(width,0),
                            new Point(width,height/2),
                            new Point(width/2,height),
                            new Point(0,height)

                        };

                        graghPath.AddLines(points);
                        using (Brush b = new SolidBrush(color2))
                        {
                            g.FillPath(b, graghPath);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);

            }


            return bmp;
        }
    }

    public class BorderViewModel : BaseBorderViewModel
    {

        public BorderViewModel(StreamViewModel stream, PropertyBorderModel border) : base(stream, border)
        {
        }
    }

    public class DesignBorderViewModel : BaseBorderViewModel
    {
        public DesignBorderViewModel(StreamViewModel stream, PropertyBorderModel border) : base(stream, border)
        {
        }
    }
}
