using MediaToolkit.Core;
using Prism.Commands;
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Drawing;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class AdvancedSettingsViewModel : BaseWindowViewModel
    {
        private readonly AdvancedSettingsModel _model;
        public override string Caption => "Advanced Settings";

        private Properties.PropertyVideoViewModel videoVeiwModel = null;

        public AdvancedSettingsViewModel(AdvancedSettingsModel model, TrackableViewModel parent) : base(parent)
        {
            _model = model;
            videoVeiwModel = ((StreamViewModel)parent).VideoViewModel;

            VideoEncoders.AddRange(EncoderHelper.GetVideoEncoderItems());
            AdjustResolutionCommand = new DelegateCommand(AdjustVideoResolution);

        }

        //[Track]
        public EncoderItem VideoEncoder
        {
            get => _model.VideoEncoder;
            set
            {
                SetProperty(_model, () => _model.VideoEncoder, value);
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
                SetProperty(_model,() => _model.Fps, value);
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

        public bool KeepAspectRatio
        {
            get => _model.KeepAspectRatio;
            set
            {

                SetProperty(_model, () => _model.KeepAspectRatio, value);

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
            {
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

            //int destHeight = (int)(destWidth / ratio);
            //if (destHeight > maxHeight)
            //{
            //    destHeight = maxHeight;
            //    destWidth = (int)(destHeight * ratio);
            //}

            //if (destHeight < minHeight)
            //{
            //    destHeight = minHeight;
            //    destWidth = (int)(destHeight * ratio);
            //}


            //if (ratio < 1)
            //{
            //    destHeight = destSize.Height;
            //    destWidth = (int)(destHeight * ratio);


            //    if (destWidth > maxWidth)
            //    {
            //        destWidth = maxWidth;
            //        destHeight = (int)(destWidth / ratio);
            //    }

            //    if (destWidth < minWidth)
            //    {
            //        destWidth = minWidth;
            //        destHeight = (int)(destWidth / ratio);
            //    }
            //}
            //else
            //{

            //}


            //this.Width = destWidth;
            //this.Height = destHeight;

        }


    }
}
