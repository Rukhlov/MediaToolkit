using System;

namespace ScreenStreamer.Wpf.ViewModels.Common
{
    public class TrackableViewModel : ViewModelBase
    {
        private static readonly Type AttrType = typeof(TrackAttribute);
        public TrackableViewModel(ViewModelBase parent = null) : base(parent, AttrType)
        {

        }
    }

    public class TrackAttribute : Attribute
    {
    }
}