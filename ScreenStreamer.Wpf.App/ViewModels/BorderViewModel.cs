using Prism.Mvvm;
using ScreenStreamer.Wpf.Common.Interfaces;

namespace ScreenStreamer.Wpf.Common.Models
{
    public abstract class BaseBorderViewModel : BindableBase, IBorderViewModel
    {
        public StreamViewModel Stream { get; }

        public BaseBorderViewModel(StreamViewModel stream)
        {
            this.Stream = stream;
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
