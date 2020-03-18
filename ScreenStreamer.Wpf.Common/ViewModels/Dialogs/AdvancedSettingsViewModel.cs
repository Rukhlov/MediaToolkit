using MediaToolkit.Core;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class AdvancedSettingsViewModel : BaseWindowViewModel
    {
        private readonly AdvancedSettingsModel _model;
        public override string Caption => "Advanced Settings";



        #region VideoEncoder
        public VideoEncoderMode VideoEncoder { get => _model.VideoEncoder; set { SetProperty(_model, () => _model.VideoEncoder, value); } }
        #endregion VideoEncoder

        #region H264Profile
        public H264Profile H264Profile { get => _model.H264Profile; set { SetProperty(_model, () => _model.H264Profile, value); } }
        #endregion H264Profile

        #region Bitrate
        public int Bitrate { get => _model.Bitrate; set { SetProperty(_model,() => _model.Bitrate, value); } }
        #endregion Bitrate

        #region MaxBitrate
        public int MaxBitrate { get => _model.MaxBitrate; set { SetProperty(_model,() => _model.MaxBitrate, value); } }
        #endregion MaxBitrate

        #region Fps
        public int Fps { get => _model.Fps; set { SetProperty(_model,() => _model.Fps, value); } }
        #endregion Fps

        #region LowLatency
        public bool LowLatency { get => _model.LowLatency; set { SetProperty(_model,() => _model.LowLatency, value); } }
        #endregion LowLatency

        public ObservableCollection<VideoEncoderMode> VideoEncoderModes { get; } = new ObservableCollection<VideoEncoderMode>()
        {
            VideoEncoderMode.H264,
            VideoEncoderMode.JPEG
        };

        public ObservableCollection<H264Profile> H264Profiles { get; } = new ObservableCollection<H264Profile>()
        {
            H264Profile.Base,
            H264Profile.Main,
            H264Profile.High
        };

        public AdvancedSettingsViewModel(AdvancedSettingsModel model)
        {
            _model = model;
            
        }


    }
}
