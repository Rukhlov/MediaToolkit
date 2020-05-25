using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using System.Linq;

namespace ScreenStreamer.Wpf.Common.Models.Properties
{
    public class PropertyQualityViewModel : PropertyBaseViewModel
    {
        private readonly PropertyQualityModel _model;
        public override string Name => "Quality";

        #region SelectedQualityPreset
        private QualityPresetViewModel _selectedQualityPreset;
        [Track]
        public QualityPresetViewModel SelectedQualityPreset
        {
            get => _selectedQualityPreset;
            set
            {
                _selectedQualityPreset = value;
                RaisePropertyChanged(() => SelectedQualityPreset);
                _model.Preset = value.Preset;
                RaisePropertyChanged(nameof(Info));
            }
        }

        #endregion SelectedQualityPreset

        public PropertyQualityViewModel(StreamViewModel parent, PropertyQualityModel model) : base(parent)
        {
            _model = model;
            _selectedQualityPreset = QualityPresetsHelper.GetQualityPresetViewModels().Find(preset => _model.Preset == preset.Preset);
        }

        public override string Info => SelectedQualityPreset.DisplayName;

        //public override object Clone()
        //{
        //    return new PropertyQualityViewModel(null, )
        //    {
        //        Info = this.Info,
        //        SelectedQualityPreset = this.SelectedQualityPreset
        //    };
        //}

        protected override IDialogViewModel BuildDialogViewModel()
        {
            return new QualitySettingsViewModel(this,Parent);
        }
    }
}
