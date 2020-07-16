using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Models.Properties;
using System.Collections.ObjectModel;
using ScreenStreamer.Wpf;

using System.Linq;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class VideoSettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Video Stream";

        public ObservableCollection<VideoSourceItem> Displays { get; set; } = new ObservableCollection<VideoSourceItem>();
        public ObservableCollection<ScreenCaptureItem> ScreenCaptures { get; set; } = new ObservableCollection<ScreenCaptureItem>();

        public System.Windows.Input.ICommand UpdateVideoSourcesCommand { get; }

        public VideoSettingsViewModel(PropertyVideoViewModel property, TrackableViewModel parent) : base(property, parent)
        {
            this.parent.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(IsChanged))
                {
                    RaisePropertyChanged(() => IsChanged);
                }
            };

            UpdateVideoSourcesCommand = new Prism.Commands.DelegateCommand(UpdateSources);

            //Displays.AddRange(ScreenHelper.GetDisplayItems());


            ScreenCaptures.AddRange(ScreenCaptureItem.SupportedCaptures);

            var streamModel = ((PropertyVideoViewModel)this.Property).Parent.Model;

            var captType = streamModel.PropertyVideo.CaptType;

            ((PropertyVideoViewModel)this.Property).CaptureType = ScreenCaptures.FirstOrDefault(c => c.CaptType == captType) ?? ScreenCaptures.FirstOrDefault();


            ////Displays.AddRange(ScreenHelper.GetScreens());
            /////
            ///
            UpdateSources();
        }

        public void UpdateSources()
        {
            Displays.Clear();

            Displays.AddRange(ScreenHelper.GetDisplayItems());

            var streamModel = ((PropertyVideoViewModel)this.Property).Parent.Model;

            var deviceId = streamModel.PropertyVideo.DeviceId;

            ((PropertyVideoViewModel)this.Property).Display = Displays.FirstOrDefault(d => d.DeviceId == deviceId) ?? Displays.FirstOrDefault();

            //((PropertyVideoViewModel)this.Property).Display = Displays.FirstOrDefault();
        }

        protected override bool CheckChanges()
        {
            return base.CheckChanges() || parent.IsChanged;
        }



        //public new bool IsClosableOnLostFocus
        //{
        //    get
        //    {
        //        return !IsChanged;
        //    }

        //}

        //=> this is PropertyWindowViewModel && !IsChanged;
    }
}
