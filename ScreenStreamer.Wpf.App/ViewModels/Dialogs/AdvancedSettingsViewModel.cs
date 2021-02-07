using MediaToolkit.Core;
using Prism.Commands;
using ScreenStreamer.Wpf.Helpers;

using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Drawing;
using System.Linq;
using ScreenStreamer.Wpf.Models;
using ScreenStreamer.Wpf.ViewModels.Properties;
using ScreenStreamer.Wpf.ViewModels.Common;

namespace ScreenStreamer.Wpf.ViewModels.Dialogs
{
    public class AdvancedSettingsViewModel : WindowViewModel
    {
        private readonly AdvancedSettingsModel _model;
		public override string Caption => LocalizationManager.GetString("AdvancedSettingsCaption");//"Advanced Settings";

        private PropertyVideoViewModel videoVeiwModel = null;

        public AdvancedSettingsViewModel(AdvancedSettingsModel model, TrackableViewModel parent) : base(parent)
        {
            _model = model;
            videoVeiwModel = ((StreamViewModel)parent).VideoViewModel;

			var appModel = ServiceLocator.GetInstance<AppModel>();
			var encoders = appModel.VideoEncoders;

			//var encoders = EncoderHelper.GetVideoEncoders();

			VideoEncoders.AddRange(encoders);

            var encoder = encoders.FirstOrDefault(e => e.Id == _model.EncoderId) ?? encoders.FirstOrDefault();
            this.VideoEncoder = encoder;

            AdjustResolutionCommand = new DelegateCommand(AdjustVideoResolution);

        }

        private EncoderItem videoEncoder = null;
        //[Track]
        public EncoderItem VideoEncoder
        {
            get => videoEncoder;
            set
            {
                //SetProperty(_model, () => _model.VideoEncoder, value);
                videoEncoder = value;

                if (_model != null)
                {
                    _model.EncoderId = videoEncoder.Id;
					_model.DriverType = videoEncoder.DriverType;
					_model.ColorSpace = videoEncoder.ColorSpace;
					_model.ColorRange = videoEncoder.ColorRange;
					_model.PixelFormat = videoEncoder.PixelFormat;

                    RaisePropertyChanged(nameof(VideoEncoder));
                }
            }
        }


       // [Track]
        public H264Profile H264Profile
        {
            get => _model.H264Profile;
            set
            {
                SetProperty(_model, () => _model.H264Profile, value);
            }
        }


       // [Track]
        public int Bitrate
        {
            get => _model.Bitrate;
            set
            {
                SetProperty(_model,() => _model.Bitrate, value);
            }
        }


       // [Track]
        public int MaxBitrate
        {
            get => _model.MaxBitrate;
            set
            {
                SetProperty(_model,() => _model.MaxBitrate, value);
            }
        }

       // [Track]
        public int Fps
        {
            get => _model.Fps;
            set
            {
                var fps = value;

                if(fps > 60)
                {
                    fps = 60;
                }
                if (fps < 1)
                {
                    fps = 1;
                }

                SetProperty(_model,() => _model.Fps, fps);
            }
        }


       // [Track]
        public bool LowLatency
        {
            get => _model.LowLatency;
            set
            {
                SetProperty(_model,() => _model.LowLatency, value);
            }
        }

        public ObservableCollection<EncoderItem> VideoEncoders { get; } = new ObservableCollection<EncoderItem>();

        public ObservableCollection<H264Profile> H264Profiles { get; } = new ObservableCollection<H264Profile>()
        {
            H264Profile.Base,
            H264Profile.Main,
            H264Profile.High
        };



        // [Track]
        public bool UseResolutionFromCaptureSource
        {
            get => _model.UseResolutionFromCaptureSource;
            set
            {
                //useResolutionFromCaptureSource = value;

                SetProperty(_model, () => _model.UseResolutionFromCaptureSource, value);

                //RaisePropertyChanged(nameof(UseResolutionFromCaptureSource));
                RaisePropertyChanged(nameof(Width));
                RaisePropertyChanged(nameof(Height));

            }
        }

      //  [Track]
        public int Width
        {
            get
            {
                if (UseResolutionFromCaptureSource)
                {
                    return videoVeiwModel.ResolutionWidth;
                }

                return _model.Width;
            }
            set
            {
                if (!UseResolutionFromCaptureSource)
                {
                    var width = value;
                    if (width < minWidth)
                    {
                        width = minWidth;
                    }
                    if (width > maxWidth)
                    {
                        width = maxWidth;
                    }

                    SetProperty(_model, () => _model.Width, width);
                }
                    
            }
        }

       // [Track]
        public int Height
        {
            get
            {
                if (UseResolutionFromCaptureSource)
                {
                    return videoVeiwModel.ResolutionHeight;
                }

                return _model.Height;
            }
            set
            {
                if (!UseResolutionFromCaptureSource)
                {
                    var height = value;
                    if ( height< minHeight)
                    {
                        height = minHeight;
                    }
                    if (height> maxHeight)
                    {
                        height = maxHeight;
                    }

                    SetProperty(_model, () => _model.Height, height);
                }
            }
        }

		public ScalingFilter DonwscaleFilter
		{
			get => _model.DownscaleFilter;
			set
			{
				SetProperty(_model, () => _model.DownscaleFilter, value);
			}
		}

		public ObservableCollection<ScalingFilter> DonwscaleFilters { get; } = new ObservableCollection<ScalingFilter>()
		{
			ScalingFilter.Point,
			ScalingFilter.FastLinear,
			ScalingFilter.Linear,
		};

		public bool KeepAspectRatio
        {
            get => _model.KeepAspectRatio;
            set
            {

                SetProperty(_model, () => _model.KeepAspectRatio, value);

            }
        }

        public bool AutoStartStreamingOnStartup
        {
            get => _model.AutoStartStreamingOnStartup;
            set
            {
                SetProperty(_model, () => _model.AutoStartStreamingOnStartup, value);
            }
        }


        public ICommand AdjustResolutionCommand { get; }

        private readonly int maxWidth = MediaToolkit.Core.Config.MaxVideoEncoderWidth;
        private readonly int minWidth = MediaToolkit.Core.Config.MinVideoEncoderWidth;
        private readonly int maxHeight = MediaToolkit.Core.Config.MaxVideoEncoderHeight;
        private readonly int minHeight = MediaToolkit.Core.Config.MinVideoEncoderHeight;

        private void AdjustVideoResolution()
        {
            var srcSize = new Size(videoVeiwModel.ResolutionWidth, videoVeiwModel.ResolutionHeight);


            var destSize = new Size(Width, Height);

            var srcRatio = srcSize.Width / (double)srcSize.Height;
            int destWidth = destSize.Width;
            int destHeight = destSize.Height;


            if (destWidth % 2 != 0)
            {// размеры должны быть четными
                destWidth--;
            }

            if (destHeight % 2 != 0)
            {
                destHeight--;
            }

            if (srcRatio > 1)
            {// ширина больше высоты самое распространенное
             // меняем ширину 
            
                if(destHeight < minHeight)
                {
                    destHeight = minHeight;
                }

                destWidth = (int)(destHeight * srcRatio);
                if (destWidth % 2 != 0)
                {
                    destWidth--;
                }

                if (destWidth > maxWidth)
                {
                    destWidth = maxWidth;
                    destHeight = (int)(destWidth / srcRatio);

                    if (destHeight % 2 != 0)
                    {
                        destHeight--;
                    }
                }

            }
            else
            { // меняем высоту

                if(destWidth < minWidth)
                {
                    destWidth = minWidth;
                }

                destHeight = (int)(destWidth / srcRatio);
                if (destHeight % 2 != 0)
                {
                    destHeight--;
                }

                if (destHeight > maxHeight)
                {
                    destHeight = maxHeight;
                    destWidth = (int)(destHeight * srcRatio);

                    if (destWidth % 2 != 0)
                    {
                        destWidth--;
                    }
                }

            }

            this.Width = destWidth;
            this.Height = destHeight;

        }


    }
}
