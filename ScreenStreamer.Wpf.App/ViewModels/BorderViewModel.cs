using Prism.Mvvm;
using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Properties;
using System.Drawing;
using static ScreenStreamer.Wpf.Common.Helpers.ScreenHelper;

namespace ScreenStreamer.Wpf.Common.Models
{
    public abstract class BaseBorderViewModel : BindableBase, IBorderViewModel
    {
        public StreamViewModel Stream { get; }
        private PropertyVideoViewModel videoViewModel = null;

        public BaseBorderViewModel(StreamViewModel stream)
        {
            this.Stream = stream;
            videoViewModel = Stream.VideoViewModel;
        }


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
                if (videoViewModel != null)
                {
                    videoViewModel.Left = (int)PointsToPixels(wpfLeft, LengthDirection.Horizontal);
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
                if (videoViewModel != null)
                {
                    videoViewModel.Top = (int)PointsToPixels(wpfTop, LengthDirection.Vertical);
                }
                

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
                if (videoViewModel != null)
                {
                    videoViewModel.ResolutionWidth = (int)PointsToPixels(wpfWidth, LengthDirection.Horizontal);
                }
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
                if (videoViewModel != null)
                {
                    videoViewModel.ResolutionHeight = (int)PointsToPixels(wpfHeight, LengthDirection.Vertical);
                } 
            }
        }

        public void SetRegion(Rectangle rect)
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
    }

    public class BorderViewModel : BaseBorderViewModel
    {

        public BorderViewModel(StreamViewModel stream) : base(stream)
        {
        }
    }

    public class DesignBorderViewModel : BaseBorderViewModel
    {
        public DesignBorderViewModel(StreamViewModel stream) : base(stream)
        {
        }
    }
}
