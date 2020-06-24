
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Models.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{

    public class QualitySettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Quality";

        public ObservableCollection<QualityPresetViewModel> Presets { get; } = new ObservableCollection<QualityPresetViewModel>();

        public QualitySettingsViewModel(PropertyQualityViewModel property, TrackableViewModel parent) : base(property, parent)
        {
            Presets.AddRange(QualityPresetsHelper.GetQualityPresetViewModels());
        }
    }

    public class QualityPresetViewModel
    {
        public string DisplayName => Preset.ToString();
        public QualityPreset Preset { get; set; }

        public override bool Equals(object obj)
        {
            var qualityPresetViewModel = obj as QualityPresetViewModel;
            if (qualityPresetViewModel == null)
            {
                return false;
            }
            return this.Preset == qualityPresetViewModel.Preset;
        }

        public override int GetHashCode()
        {
            return this.Preset.GetHashCode();
        }
    }

    public static class QualityPresetsHelper
    {
        public static List<QualityPresetViewModel> GetQualityPresetViewModels()
        {
            return new List<QualityPresetViewModel>()
            {
                new QualityPresetViewModel{Preset = QualityPreset.Low},
                new QualityPresetViewModel{Preset = QualityPreset.Standard},
                new QualityPresetViewModel{Preset = QualityPreset.High},
            };
        }
    }
}
