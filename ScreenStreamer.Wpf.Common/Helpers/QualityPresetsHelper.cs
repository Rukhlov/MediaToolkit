using ScreenStreamer.Wpf.Common.Models;
using System.Collections.Generic;

namespace ScreenStreamer.Wpf.Common.Helpers
{
    public static class QualityPresetsHelper
    {
        public static List<QualityPresetViewModel> GetQualityPresetViewModels()
        {
            return new List<QualityPresetViewModel>()
            {
                new QualityPresetViewModel{Preset = Enums.QualityPreset.Low},
                new QualityPresetViewModel{Preset = Enums.QualityPreset.Standard},
                new QualityPresetViewModel{Preset = Enums.QualityPreset.High},
            };
        }
    }
}
