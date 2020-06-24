using System;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class TrackableViewModel : Polywall.Share.UI.ViewModelBase
    {
        private static readonly Type AttrType = typeof(TrackAttribute);
        public TrackableViewModel(Polywall.Share.UI.ViewModelBase parent = null) : base(parent, AttrType)
        {

        }
    }

    public class TrackAttribute : Attribute
    {
    }
}