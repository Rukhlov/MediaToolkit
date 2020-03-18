using ScreenStreamer.Wpf.Common.Enums;

namespace ScreenStreamer.Wpf.Common.Models
{
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
}
