using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Models.Properties;
using System.Collections.ObjectModel;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class QualitySettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Quality";

        public ObservableCollection<QualityPresetViewModel> Presets { get; } = new ObservableCollection<QualityPresetViewModel>();

        public QualitySettingsViewModel(PropertyQualityViewModel property) : base(property)
        {
            Presets.AddRange(QualityPresetsHelper.GetQualityPresetViewModels());
        }
    }
}
