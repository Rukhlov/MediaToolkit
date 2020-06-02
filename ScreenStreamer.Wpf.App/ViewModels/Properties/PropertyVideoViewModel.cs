using System;
using System.Drawing;
using ScreenStreamer.Wpf.Common.Enums;
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Input;
//using Polywall.Share.Exceptions;
using Prism.Commands;
using MediaToolkit.UI;

namespace ScreenStreamer.Wpf.Common.Models.Properties
{
    public class PropertyVideoViewModel : PropertyBaseViewModel
    {
        private readonly PropertyVideoModel _model;
        public override string Name => "Video";

        public ICommand ShowBorderCommand { get; }


        [Track]
        public bool IsRegion
        {
            get => _model.IsRegion;
            set
            {
                SetProperty(_model, () => _model.IsRegion, value);

                UpdateRegion();
                RaisePropertyChanged(nameof(Info));
            }
        }

        //private (double Top, double Left, double Width, double Height) GetRegion()
        //{

        //    if (IsRegion)
        //    {
        //        return (this.Top, this.Left, this.ResolutionWidth, this.ResolutionHeight);
        //    }
        //    else
        //    {
        //        var rect = ScreenHelper.GetScreenBounds(Display) ?? new Rectangle();
        //        return (rect.Top, rect.Left, rect.Width, rect.Height);
        //    }
        //}


        //[Track]
        //public string Display
        //{
        //    get => _model.Display?.Name ?? "";
        //    set
        //    {
        //        SetProperty(_model, () => _model.Display, value);

        //        UpdateRegion();
        //        RaisePropertyChanged(nameof(Info));
        //    }
        //}



        [Track]
        public VideoSourceItem Display
        {
            get => _model.Display;
            set
            {

                SetProperty(_model, () => _model.Display, value);

                
                UpdateRegion();
                RaisePropertyChanged(nameof(Info));
            }
        }



        [Track]
        public int ResolutionHeight
        {
            get => _model.ResolutionHeight;
            set
            {
                SetProperty(_model, () => _model.ResolutionHeight, value);
            }
        }



        [Track]
        public int ResolutionWidth
        {
            get => _model.ResolutionWidth;
            set
            {
                SetProperty(_model, () => _model.ResolutionWidth, value);
            }
        }


        [Track]
        public string Resolution
        {
            get => (_model.ResolutionWidth + "x" + _model.ResolutionHeight);

        }



        [Track]
        public int Top
        {
            get => _model.Top;
            set
            {
                SetProperty(_model, () => _model.Top, value);
            }
        }

        [Track]
        public int Left
        {
            get => _model.Left;
            set
            {
                SetProperty(_model, () => _model.Left, value);
            }
        }



        [Track]
        public bool AspectRatio
        {
            get => _model.AspectRatio;
            set
            {
                SetProperty(_model, () => _model.AspectRatio, value);
            }
        }



        public ICommand AdjustCommand { get; }


        public PropertyVideoViewModel(StreamViewModel parent, PropertyVideoModel model) : base(parent)
        {
            _model = model;
            ShowBorderCommand = new DelegateCommand(ShowBorder);
            AdjustCommand = new DelegateCommand(Adjust);
        }


        private SelectAreaForm selectAreaForm = null;
        private void UpdateRegion()
        {
            if(!IsRegion)
            {
                var region =  Display?.CaptureRegion ?? Rectangle.Empty;

                this.Top = region.Top;
                this.Left = region.Left;
                this.ResolutionHeight = region.Height;
                this.ResolutionWidth = region.Width;

            }


            //var sourceItem = _model.Display;
            //bool isRegionItem = sourceItem.DeviceId == "ScreenRegion";
            //if (isRegionItem)
            //{
            //    if (selectAreaForm == null)
            //    {
            //        selectAreaForm = new SelectAreaForm();
            //    }
            //}
            //if (selectAreaForm != null)
            //{
            //    selectAreaForm.Visible = isRegionItem;
            //}
        }

        private void Adjust()
        {

        }

        private void ShowBorder()
        {
            var model = Parent.Properties.OfType<PropertyBorderViewModel>().FirstOrDefault();
            if (model != null)
            {
                model.IsBorderVisible = true;
            }

            //Parent.Properties.OfType<PropertyBorderViewModel>().FirstOrDefault()
            //    .Do(model => model.IsBorderVisible = true);
        }

        protected override IDialogViewModel BuildDialogViewModel()
        {
            return new VideoSettingsViewModel(this,Parent);
        }

        public override string Info
        {
            get
            {
                var builder = new StringBuilder();
                if (IsRegion)
                {
                    builder.Append($"{nameof(this.Top)}: {this.Top}, {nameof(this.Left)}: {this.Left}, Width: {this.ResolutionWidth}, Height: {this.ResolutionHeight}");
                }

                else
                {

                    builder.Append(this.Display?.Name);
                }
 

                if (builder.Length > MaxInfoLength)
                {
                    return builder.ToString(0, MaxInfoLength - 3) + "...";
                }
                return builder.ToString();
            }
        }
    }
}
