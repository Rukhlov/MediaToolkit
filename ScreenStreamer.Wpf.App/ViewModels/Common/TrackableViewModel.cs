using System;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
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