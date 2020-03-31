using MediaToolkit.Core;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class AdvancedSettingsViewModel : BaseWindowViewModel
    {
        private readonly AdvancedSettingsModel _model;
        public override string Caption => "Advanced Settings";

        public VideoEncoderMode VideoEncoder
        {
            get => _model.VideoEncoder;
            set
            {
                SetProperty(_model, () => _model.VideoEncoder, value);
            }
        }

        public H264Profile H264Profile
        {
            get => _model.H264Profile;
            set
            {
                SetProperty(_model, () => _model.H264Profile, value);
            }
        }

        public int Bitrate
        {
            get => _model.Bitrate;
            set
            {
                SetProperty(_model,() => _model.Bitrate, value);
            }
        }


        public int MaxBitrate
        {
            get => _model.MaxBitrate;
            set
            {
                SetProperty(_model,() => _model.MaxBitrate, value);
            }
        }


        public int Fps
        {
            get => _model.Fps;
            set
            { SetProperty(_model,() => _model.Fps, value);
            }
        }


        public bool LowLatency
        {
            get => _model.LowLatency;
            set
            { SetProperty(_model,() => _model.LowLatency, value);
            }
        }

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
