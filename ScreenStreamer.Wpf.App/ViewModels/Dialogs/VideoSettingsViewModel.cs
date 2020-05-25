using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Models.Properties;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class VideoSettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Video Stream";
        public ObservableCollection<string> Displays { get; set; } = new ObservableCollection<string>();

        public VideoSettingsViewModel(PropertyVideoViewModel property, StreamerViewModelBase parent) : base(property, parent)
        {
            this.parent.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(IsChanged))
                {
                    RaisePropertyChanged(() => IsChanged);
                }
            };
            Displays.AddRange(ScreenHelper.GetScreens());
        }

        protected override bool CheckChanges()
        {
            return base.CheckChanges() || parent.IsChanged;
        }

    }
}
