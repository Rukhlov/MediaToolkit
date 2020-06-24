using MediaToolkit.Core;
using Prism.Commands;
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class AdvancedSettingsViewModel : BaseWindowViewModel
    {
        private readonly AdvancedSettingsModel _model;
        public override string Caption => "Advanced Settings";


        //[Track]
        //public VideoCodingFormat VideoEncoder
        //{
        //    get => _model.VideoEncoder;
        //    set
        //    {
        //        SetProperty(_model, () => _model.VideoEncoder, value);
        //    }
        //}

        [Track]
        public EncoderItem VideoEncoder
        {
            get => _model.VideoEncoder;
            set
            {
                SetProperty(_model, () => _model.VideoEncoder, value);
            }
        }


        [Track]
        public H264Profile H264Profile
        {
            get => _model.H264Profile;
            set
            {
                SetProperty(_model, () => _model.H264Profile, value);
            }
        }


        [Track]
        public int Bitrate
        {
            get => _model.Bitrate;
            set
            {
                SetProperty(_model,() => _model.Bitrate, value);
            }
        }


        [Track]
        public int MaxBitrate
        {
            get => _model.MaxBitrate;
            set
            {
                SetProperty(_model,() => _model.MaxBitrate, value);
            }
        }

        [Track]
        public int Fps
        {
            get => _model.Fps;
            set
            {
                SetProperty(_model,() => _model.Fps, value);
            }
        }


        [Track]
        public bool LowLatency
        {
            get => _model.LowLatency;
            set
            {
                SetProperty(_model,() => _model.LowLatency, value);
            }
        }

        public ObservableCollection<EncoderItem> VideoEncoders { get; } = new ObservableCollection<EncoderItem>();

        //public ObservableCollection<VideoCodingFormat> VideoEncoderModes { get; } = new ObservableCollection<VideoCodingFormat>()
        //{
        //    VideoCodingFormat.H264,
        //    VideoCodingFormat.JPEG
        //};

        public ObservableCollection<H264Profile> H264Profiles { get; } = new ObservableCollection<H264Profile>()
        {
            H264Profile.Base,
            H264Profile.Main,
            H264Profile.High
        };

        public AdvancedSettingsViewModel(AdvancedSettingsModel model, TrackableViewModel parent) :base(parent)
        {
            _model = model;


            VideoEncoders.AddRange(EncoderHelper.GetVideoEncoderItems());


        }


    }
}
