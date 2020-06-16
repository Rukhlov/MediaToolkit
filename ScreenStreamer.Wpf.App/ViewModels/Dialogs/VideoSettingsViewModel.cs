using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Models.Properties;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class VideoSettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Video Stream";
        public ObservableCollection<VideoSourceItem> Displays { get; set; } = new ObservableCollection<VideoSourceItem>();


        public ObservableCollection<ScreenCaptureType> ScreenCaptures { get; set; } = new ObservableCollection<ScreenCaptureType>();

        //public ObservableCollection<string> Displays { get; set; } = new ObservableCollection<string>();

        public System.Windows.Input.ICommand UpdateVideoSourcesCommand { get; }



        public VideoSettingsViewModel(PropertyVideoViewModel property, StreamerViewModelBase parent) : base(property, parent)
        {
            this.parent.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(IsChanged))
                {
                    RaisePropertyChanged(() => IsChanged);
                }
            };

            UpdateVideoSourcesCommand = new Prism.Commands.DelegateCommand(UpdateSources);

            Displays.AddRange(ScreenHelper.GetDisplayItems());

            ScreenCaptures.AddRange(ScreenCaptureType.SupportedCaptures);

            //Displays.AddRange(ScreenHelper.GetScreens());
        }

        public void UpdateSources()
        {
            Displays.Clear();

            Displays.AddRange(ScreenHelper.GetDisplayItems());

            ((PropertyVideoViewModel)this.Property).Display = Displays.FirstOrDefault();
        }

        protected override bool CheckChanges()
        {
            return base.CheckChanges() || parent.IsChanged;
        }

    }
}
