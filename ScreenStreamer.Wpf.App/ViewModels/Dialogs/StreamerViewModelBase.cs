using System;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class StreamerViewModelBase : Polywall.Share.UI.ViewModelBase
    {
        private static readonly Type AttrType = typeof(TrackAttribute);
        public StreamerViewModelBase(Polywall.Share.UI.ViewModelBase parent = null) : base(parent, AttrType)
        {

        }
    }

    public class TrackAttribute : Attribute
    {
    }
}